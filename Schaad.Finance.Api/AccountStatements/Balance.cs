using System;

namespace Schaad.Finance.Api.AccountStatements
{
    /// <summary>
    /// Detailed balance info
    /// </summary>
    public class Balance
    {
        /// <summary>
        /// Date of balance
        /// </summary>
        public DateTime BookingDate { get; set; }

        /// <summary>
        /// Currency of balance
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public double Value { get; set; }
    }
}
