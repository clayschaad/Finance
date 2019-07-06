using Schaad.Finance.Api.AccountStatements;
using System.Collections.Generic;
using System.Text;

namespace Schaad.Finance.Api
{
    public interface IAccountStatementParsingService
    {
        List<AccountStatement> ReadFile(string filePath, Encoding encoding);
        string ValidateAccountStatement(AccountStatement accountStatement);
    }
}