using CsvHelper.Configuration;
using Schaad.Finance.Api.Datasets;

namespace Schaad.Finance.Formats.CreditCardProvider
{
    class CembraMap : ClassMap<CreditCardTransaction>
    {
        public CembraMap()
        {
            Map(m => m.BookingDate).Name("Booking Date");
            Map(m => m.TransactionDate).Name("Transaction Date");
            Map(m => m.Transaction).Name("Transaction");
            Map(m => m.TransactionType).Name("Type");
            Map(m => m.Amount).Name("Amount (CHF)");
        }
    }
}