namespace Schaad.Finance.Api.AccountStatements
{
    public class AccountStatementResult
    {
        public readonly AccountStatement AccountStatement;

        public readonly bool IsSuccess;

        public readonly string Error;

        public AccountStatementResult(AccountStatement accountStatement, string error)
        {
            AccountStatement = accountStatement;
            Error = error;
            IsSuccess = string.IsNullOrEmpty(error);
        }
    }
}
