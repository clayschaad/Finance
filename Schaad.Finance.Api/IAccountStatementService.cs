using Schaad.Finance.Api.AccountStatements;
using System.Collections.Generic;
using System.Text;

namespace Schaad.Finance.Api
{
    public interface IAccountStatementService
    {
        List<AccountStatementResult> ReadFile(string filePath, Encoding encoding);
    }
}