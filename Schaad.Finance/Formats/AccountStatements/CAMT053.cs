using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Schaad.Finance.Api;
using Schaad.Finance.Api.AccountStatements;
using Schaad.Finance.Formats.AccountStatements.Models;

namespace Schaad.Finance.Formats.AccountStatements
{
    /// <summary>
    /// Reads CAMT053 format
    /// https://www.six-interbank-clearing.com/dam/downloads/de/standardization/iso/swiss-recommendations/implementation-guidelines-camt.pdf
    /// </summary>
    public class CAMT053 : IAccountStatementParsingService
    {
        public List<AccountStatement> ReadFile(string filePath, Encoding encoding)
        {
            var accountStatements = new List<AccountStatement>();
            var serializer = new XmlSerializer(typeof(Document));
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                using (var reader = new StreamReader(fileStream, encoding))
                {
                    var document = (Document)serializer.Deserialize(reader);
                    foreach (var stmt in document.BkToCstmrStmt.Stmt)
                    {
                        var accountStatement = new AccountStatement();
                        accountStatements.Add(accountStatement);

                        accountStatement.AccountNumber = stmt.Acct.Id.Item.ToString();
                        accountStatement.Currency = stmt.Acct.Ccy;

                        foreach (var balance in stmt.Bal)
                        {
                            // var balanceAmount =  balance.Amt.Value;
                            // var balanceCurrency = balance.Amt.Ccy;
                            // var balanceDate = balance.Dt.Item;
                            // var creditDebit = balance.CdtDbtInd;

                            var balanceType = (BalanceType12Code)balance.Tp.CdOrPrtry.Item;
                            if (balanceType == BalanceType12Code.OPBD)
                            {
                                accountStatement.StartBalance = new Balance
                                {
                                    BookingDate = balance.Dt.Item,
                                    Value = (double)balance.Amt.Value,
                                };
                            }
                            else if (balanceType == BalanceType12Code.CLBD)
                            {
                                accountStatement.EndBalance = new Balance
                                {
                                    BookingDate = balance.Dt.Item,
                                    Value = (double)balance.Amt.Value,
                                };
                            }
                        }

                        if (stmt.TxsSummry != null)
                        {
                            if (stmt.TxsSummry.TtlCdtNtries != null)
                            {
                                accountStatement.CreditTransactionSummary = new TransactionSummary
                                {
                                    NumberOfTransaction = int.Parse(stmt.TxsSummry.TtlCdtNtries.NbOfNtries),
                                    Sum = stmt.TxsSummry.TtlCdtNtries.Sum
                                };
                            }
                            else
                            {
                                accountStatement.CreditTransactionSummary = new TransactionSummary();
                            }

                            if (stmt.TxsSummry.TtlDbtNtries != null)
                            {
                                accountStatement.DebitTransactionSummary = new TransactionSummary
                                {
                                    NumberOfTransaction = int.Parse(stmt.TxsSummry.TtlDbtNtries.NbOfNtries),
                                    Sum = stmt.TxsSummry.TtlDbtNtries.Sum
                                };
                            }
                            else
                            {
                                accountStatement.DebitTransactionSummary = new TransactionSummary();
                            }
                        }

                        accountStatement.Transactions = new List<Transaction>();
                        if (stmt.Ntry != null)
                        {
                            foreach (var entry in stmt.Ntry)
                            {
                                var transaction = new Transaction();
                                accountStatement.Transactions.Add(transaction);

                                // var amount = entry.Amt.Value;
                                // var currency = entry.Amt.Ccy;
                                // var valueDate = entry.ValDt.Item;
                                // var bookingDate = entry.BookgDt.Item;
                                //var creditDepit = entry.CdtDbtInd;
                                // var trxId = entry.NtryRef;
                                // var text = entry.AddtlNtryInf;

                                transaction.Id = stmt.Id + entry.NtryRef;
                                transaction.BookingDate = entry.BookgDt.Item;
                                transaction.Text = ParseEntry(entry);
                                transaction.Value = (double)entry.Amt.Value;
                                transaction.ValueDate = entry.ValDt.Item;

                                if (entry.CdtDbtInd == CreditDebitCode.DBIT)
                                {
                                    transaction.Value *= -1;
                                }
                            }
                        }
                    }
                }
            }

            return accountStatements;
        }

        private string ParseEntry(ReportEntry4 entry)
        {
            var set = new HashSet<string>();
            foreach (var addtlNtryInf in SplitForNewline(entry.AddtlNtryInf).Where(a => !string.IsNullOrEmpty(a)))
            {
                set.Add(addtlNtryInf);
            }
            foreach (var ntryDtl in entry.NtryDtls)
            {
                foreach (var txDtl in ntryDtl.TxDtls)
                {
                    if (txDtl.RmtInf?.Ustrd != null)
                    {
                        foreach (var ustrd in txDtl.RmtInf.Ustrd)
                        {
                            foreach (var info in SplitForNewline(ustrd).Where(a => !string.IsNullOrEmpty(a)))
                            {
                                set.Add(info.Trim());
                            }
                        }
                    }
                }
            }

            return string.Join(" / ", set);
        }

        private IReadOnlyList<string> SplitForNewline(string text)
        {
            return text.Split('\n');
        }

        public string ValidateAccountStatement(AccountStatement accountStatement)
        {
            if (accountStatement.StartBalance.Currency != accountStatement.EndBalance.Currency)
            {
                return $"Startbalance currency ({accountStatement.StartBalance.Currency}) does not match endbalance currency ({accountStatement.EndBalance.Currency})";
            }

            if (accountStatement.CreditTransactionSummary != null && accountStatement.DebitTransactionSummary != null)
            {
                var transactionSum = Math.Round(accountStatement.Transactions.Sum(t => (decimal)t.Value), 2);
                var totalNumber = accountStatement.Transactions.Count;

                var transactionSumValidation = accountStatement.CreditTransactionSummary.Sum - accountStatement.DebitTransactionSummary.Sum;
                var totalNumberValidation = accountStatement.CreditTransactionSummary.NumberOfTransaction + accountStatement.DebitTransactionSummary.NumberOfTransaction;

                if (transactionSum != transactionSumValidation)
                {
                    return $"Transaction sum ({transactionSum}) does not match validation sum ({transactionSumValidation})";
                }

                if (totalNumber != totalNumberValidation)
                {
                    return $"Transaction count ({totalNumber}) does not match validation count ({totalNumberValidation})";
                }
            }
            else if (accountStatement.StartBalance != null && accountStatement.EndBalance != null)
            {
                var transactionSum = Math.Round(accountStatement.Transactions.Sum(t => t.Value), 2);
                var end = accountStatement.StartBalance.Value + transactionSum;
                if (end != accountStatement.EndBalance.Value)
                {
                    return $"Enbalance ({accountStatement.EndBalance.Value}) does not match startbalance ({accountStatement.StartBalance.Value}) + transactions ({transactionSum})";
                }
            }

            return string.Empty;
        }
    }
}