using System;

namespace Schaad.Finance.Api.Datasets
{
    public class CreditCardTransaction
    {
        public DateTime BookingDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Transaction { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
    }
}
