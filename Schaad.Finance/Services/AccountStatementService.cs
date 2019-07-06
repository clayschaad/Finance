using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Schaad.Finance.Api;
using Schaad.Finance.Api.AccountStatements;
using Schaad.Finance.Formats.AccountStatements;

namespace Schaad.Finance.Services
{
    public class AccountStatementService : IAccountStatementService
    {
        public List<AccountStatementResult> ReadFile(string filePath, Encoding encoding)
        {
            IAccountStatementParsingService accountStatementParsingService = null;
            var extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".xml":
                    accountStatementParsingService = new CAMT053() as IAccountStatementParsingService;
                    break;

                case ".sta":
                    accountStatementParsingService = new MT940() as IAccountStatementParsingService;
                    break;

                default:
                    throw new NotImplementedException($"Unknwon ending {extension}");
            }

            var accountStatementResults = new List<AccountStatementResult>();
            var accountStatements = accountStatementParsingService.ReadFile(filePath, encoding);
            foreach (var accountStatement in accountStatements)
            {
                var error = accountStatementParsingService.ValidateAccountStatement(accountStatement);
                var accountStatementResult = new AccountStatementResult(accountStatement, error);
                accountStatementResults.Add(accountStatementResult);
            }

            return accountStatementResults;
        }
    }
}
