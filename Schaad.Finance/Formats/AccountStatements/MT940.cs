using Schaad.Finance.Api;
using Schaad.Finance.Api.AccountStatements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Schaad.Finance.Formats.AccountStatements
{
    /// <summary>
    /// Reads MT940 format
    /// http://www.kontopruef.de/mt940s.shtml
    /// </summary>
    public class MT940 : IAccountStatementParsingService
    {
        public List<AccountStatement> ReadFile(string filePath, Encoding encoding)
        {
            var accountList = new List<AccountStatement>();
            AccountStatement account = null;
            Transaction currentTransaction = null;
            List<string> transactionTexts = null;
            foreach (var line in File.ReadLines(filePath, encoding))
            {
                if (line.Length < 3)
                {
                    continue;
                }

                var codeValue = ParseLine(line);
                switch (codeValue.Item1)
                {
                    // Account
                    case ":25:":
                        account = new AccountStatement();
                        accountList.Add(account);
                        account.AccountNumber = codeValue.Item2;
                        break;
                    // Start balance
                    case ":60F:":
                        account.StartBalance = ParseBalance(codeValue.Item2);
                        break;
                    // End balance
                    case ":62F:":
                        account.EndBalance = ParseBalance(codeValue.Item2);
                        break;
                    // Transaction
                    case ":61:":
                        // have we an old transaction
                        if (currentTransaction != null && transactionTexts.Any())
                        {
                            currentTransaction.Text = string.Join(", ", transactionTexts);
                        }
                        transactionTexts = new List<string>();
                        currentTransaction = ParseTransaction(codeValue.Item2);
                        account.Transactions.Add(currentTransaction);
                        break;
                    // Transaction text
                    case ":NS:":
                        //if (currentTransaction != null) currentTransaction.Text = codeValue.Item2;
                        break;
                }
                if (codeValue.Item1.IndexOf(':') == -1 && currentTransaction != null)
                {
                    if (transactionTexts.Contains(codeValue.Item2) == false)
                    {
                        transactionTexts.Add(codeValue.Item2);
                    }
                }
            }
            // have we an old transaction
            if (currentTransaction != null && transactionTexts.Any())
            {
                currentTransaction.Text = string.Join(", ", transactionTexts);
            }

            return accountList;
        }

        public string ValidateAccountStatement(AccountStatement accountStatement)
        {
            if (accountStatement.StartBalance.Currency != accountStatement.EndBalance.Currency)
            {
                return $"Startbalance currency ({accountStatement.StartBalance.Currency}) does not match endbalance currency ({accountStatement.EndBalance.Currency})";
            }

            var transactionSum = Math.Round(accountStatement.Transactions.Sum(t => t.Value), 2);
            var end = accountStatement.StartBalance.Value + transactionSum;
            if (end != accountStatement.EndBalance.Value)
            {
                return $"Enbalance ({accountStatement.EndBalance.Value}) does not match startbalance ({accountStatement.StartBalance.Value}) + transactions ({transactionSum})";
            }

            return string.Empty;
        }

        /// <summary>
        /// Parse a line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private Tuple<string, string> ParseLine(string line)
        {
            if (line.StartsWith(":"))
            {
                var pos = line.IndexOf(':', 1);
                var code = line.Substring(0, pos + 1);
                return new Tuple<string, string>(code, line.Substring(code.Length));
            }
            else
                return new Tuple<string, string>(line.Substring(0, 2), line.Substring(2));
        }

        /// <summary>
        /// Parse a balance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Balance ParseBalance(string value)
        {
            var balance = new Balance();

            bool isDebit = false;
            if (value.First() == 'C')
                isDebit = false;
            else if (value.First() == 'D')
                isDebit = true;
            value = value.Remove(0, 1);

            balance.BookingDate = ParseDate(value.Substring(0, 6));
            value = value.Remove(0, 6);

            balance.Currency = value.Substring(0, 3);
            value = value.Remove(0, 3);

            balance.Value = ParseBalanceValue(value);

            if (isDebit)
                balance.Value *= -1.0;

            return balance;
        }

        private double ParseBalanceValue(string balanceValue)
        {
            return Double.Parse(balanceValue.Replace(',', '.'));
        }

        /// <summary>
        /// Parse a transaction
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Transaction ParseTransaction(string value)
        {
            var transaction = new Transaction();

            transaction.ValueDate = ParseDate(value.Substring(0, 6));
            value = value.Remove(0, 6);

            string year = (transaction.ValueDate.Year - 2000).ToString();
            transaction.BookingDate = ParseDate(year + value.Substring(0, 4));
            value = value.Remove(0, 4);

            bool isDebit = false;
            if (value.First() == 'C')
                isDebit = false;
            else if (value.First() == 'D')
                isDebit = true;
            else
                throw new NotSupportedException("Code not supported: " + value.First());
            value = value.Remove(0, 1);

            transaction.Value = ParseBalanceValue(value.Substring(0, 15));
            value = value.Remove(0, 15);

            if (isDebit)
                transaction.Value *= -1.0;

            return transaction;
        }

        /// <summary>
        /// Parse a date in format: JJMMTT
        /// </summary>
        /// <param name="dateValue"></param>
        /// <returns></returns>
        private DateTime ParseDate(string dateValue)
        {
            if (dateValue.Length != 6)
                throw new Exception("Wrong format for date. Need: JJMMTT");
            return new DateTime(2000 + Int32.Parse(dateValue.Substring(0, 2)), Int32.Parse(dateValue.Substring(2, 2)), Int32.Parse(dateValue.Substring(4, 2)));
        }
    }
}