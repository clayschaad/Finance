using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Schaad.Finance.Api;
using Schaad.Finance.Api.Datasets;

namespace Schaad.Finance.Services
{
    public class CreditCardStatementService : ICreditCardStatementService
    {
        private readonly IPdfParsingService pdfParsingService;

        public CreditCardStatementService(IPdfParsingService pdfParsingService)
        {
            this.pdfParsingService = pdfParsingService;
        }

        public IReadOnlyList<CreditCardTransaction> ReadFile(CreditCardProvider creditCardProvider, string filePath, Encoding encoding)
        {
            switch (creditCardProvider)
            {
                case CreditCardProvider.CembraCsv:
                    return ParseCembraCsv(filePath, encoding);

                case CreditCardProvider.CembraPdf:
                    return ParseCembraPdf(filePath);

                default:
                    throw new NotImplementedException($"Unnknown procider {creditCardProvider}");
            }
        }

        private IReadOnlyList<CreditCardTransaction> ParseCembraCsv(string filePath, Encoding encoding)
        {
            using (var reader = new StreamReader(filePath, encoding))
            {
                using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
                {
                    csv.Configuration.RegisterClassMap<Formats.CreditCardProvider.CembraMap>();
                    var records = csv.GetRecords<CreditCardTransaction>();
                    return records.ToList();
                }
            }
        }

        private IReadOnlyList<CreditCardTransaction> ParseCembraPdf(string filePath)
        {
            var transactionList = new List<CreditCardTransaction>();
            var totalPages = pdfParsingService.GetTotalPages(filePath);

            for (int i = 2; i < totalPages; i++)
            {
                transactionList.AddRange(ParseCembraPdfPage(filePath, i));
            }

            return transactionList;
        }

        private IReadOnlyList<CreditCardTransaction> ParseCembraPdfPage(string filePath, int page)
        {
            var text = pdfParsingService.ExtractText(filePath, page);

            var transactionList = new List<CreditCardTransaction>();

            var startReading = false;
            foreach (var line in text)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("Einkaufs-Datum"))
                {
                    startReading = true;
                    continue;
                }

                if (trimmedLine.StartsWith("Saldo per"))
                {
                    break;
                }

                if (startReading)
                {
                    var transaction = ParseLine(trimmedLine);
                    if (transaction != null)
                    {
                        transactionList.Add(transaction);
                    }
                }
            }

            return transactionList;
        }

        private CreditCardTransaction ParseLine(string line)
        {
            var nextPart = GetNextPart(line, 10);

            if (nextPart.part == null)
            {
                return null;
            }

            var purchaseDate = ParseDateTime(nextPart.part);

            if (purchaseDate.HasValue == false)
            {
                return null;
            }

            nextPart = GetNextPart(nextPart.rest, 10);
            var bookingDate = ParseDateTime(nextPart.part);

            if (bookingDate.HasValue == false)
            {
                return null;
            }

            nextPart = GetNextPartUntilSpace(nextPart.rest, 2);
            var transactionText = nextPart.part;

            nextPart = GetNextPartUntilSpace(nextPart.rest, 2);
            var amount = nextPart.part.Replace("'", "");

            return new CreditCardTransaction
            {
                BookingDate = bookingDate.Value,
                TransactionDate = purchaseDate.Value,
                Amount = decimal.Parse(amount),
                Transaction = transactionText
            };
        }

        private (string part, string rest) GetNextPart(string line, int length)
        {
            if (line.Length < length)
            {
                return (null, null);
            }

            var part = line.Substring(0, length);
            var rest = line.Substring(length, line.Length - length).TrimStart();
            return (part, rest);
        }

        private (string part, string rest) GetNextPartUntilSpace(string line, int howManySpaces)
        {
            var part = "";
            var spaceCount = 0;
            foreach (var c in line)
            {
                if (c == ' ')
                {
                    spaceCount++;
                }
                else
                {
                    spaceCount = 0;
                }

                part += c;

                if (spaceCount == howManySpaces)
                {
                    part = part.Trim();
                    break;
                }
            }

            var rest = line.Substring(part.Length, line.Length - part.Length).TrimStart();
            return (part, rest);
        }

        private DateTime? ParseDateTime(string dateString)
        {
            var deCH = new CultureInfo("de-CH");
            DateTime purchaseDate;
            if (DateTime.TryParseExact(dateString, "dd.MM.yyyy", deCH, DateTimeStyles.None, out purchaseDate))
            {
                return purchaseDate;
            }
            return null;
        }
    }
}
