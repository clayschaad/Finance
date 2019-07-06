using System.Collections.Generic;

namespace Schaad.Finance.Api.AccountStatements
{
    public class AccountStatement
    {
        /// <summary>
        /// The account number (IBAN)
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Account Currency
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The starting balance
        /// </summary>
        public Balance StartBalance { get; set; }

        /// <summary>
        /// The end balance
        /// </summary>
        public Balance EndBalance { get; set; }

        /// <summary>
        /// Summary of all credit transactions (camt)
        /// </summary>
        public TransactionSummary CreditTransactionSummary { get; set; }

        /// <summary>
        /// Summary of all debit transactions (camt)
        /// </summary>
        public TransactionSummary DebitTransactionSummary { get; set; }

        /// <summary>
        /// A list of transactions
        /// </summary>
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
