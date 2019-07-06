using Schaad.Finance.Api.Datasets;
using System.Collections.Generic;
using System.Text;

namespace Schaad.Finance.Api
{
    public interface ICreditCardStatementService
    {
        IReadOnlyList<CreditCardTransaction> ReadFile(CreditCardProvider creditCardProvider, string filePath, Encoding encoding);
    }
}