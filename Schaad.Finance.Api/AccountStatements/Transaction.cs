using System;
using System.Security.Cryptography;
using System.Text;

namespace Schaad.Finance.Api.AccountStatements
{
    /// <summary>
    /// A transaction
    /// </summary>
    public class Transaction
    {
        public string Id 
        { 
            set 
            {
                id = value;
            }
            get 
            {
                if (string.IsNullOrEmpty(id))
                {
                    return GetHashString(ValueDate.ToString("yyyyMMdd") + Value + Text);
                }
                else 
                {
                    return id;
                }
            }
        }
        private string id;

        public DateTime ValueDate { get; set; }

        public DateTime BookingDate { get; set; }

        public double Value { get; set; }

        public string Text { get; set; }

        private string GetHashString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        private byte[] GetHash(string inputString)
        {
            var algorithm = MD5.Create(); //or use SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
    }
}
