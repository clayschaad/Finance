using System;

namespace Schaad.Finance.Api
{
    public interface IFxService
    {
        decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency, string fixerIoApiKey);
        decimal GetFxRate(string fromCurrency, string toCurrency, DateTime date, string fixerIoApiKey);
    }
}