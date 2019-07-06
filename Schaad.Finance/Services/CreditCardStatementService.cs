using System;
using System.Collections.Generic;
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
        public IReadOnlyList<CreditCardTransaction> ReadFile(CreditCardProvider creditCardProvider, string filePath, Encoding encoding)
        {
            switch (creditCardProvider)
            {
                case CreditCardProvider.Cembra:
                    return ParseCembra(filePath, encoding);

                default:
                    throw new NotImplementedException($"Unnknown procider {creditCardProvider}");
            }
        }

        private IReadOnlyList<CreditCardTransaction> ParseCembra(string filePath, Encoding encoding)
        {
            using (var reader = new StreamReader(filePath, encoding))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.RegisterClassMap<Formats.CreditCardProvider.CembraMap>();
                    var records = csv.GetRecords<CreditCardTransaction>();
                    return records.ToList();
                }
            }
        }
    }
}
