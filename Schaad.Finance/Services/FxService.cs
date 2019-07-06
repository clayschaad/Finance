using Newtonsoft.Json.Linq;
using Schaad.Finance.Api;
using System;
using System.Collections.Generic;
using System.Net;

namespace Schaad.Finance.Services
{
    public class FxService : IFxService
    {
        private Dictionary<string, decimal> fxRateCache;

        public FxService()
        {
            fxRateCache = new Dictionary<string, decimal>();
        }

        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency, string fixerIoApiKey)
        {
            if (fromCurrency == toCurrency)
            {
                return amount;
            }

            var fxRate = GetFxRate(fromCurrency, toCurrency, DateTime.Now, fixerIoApiKey);
            return Math.Round(fxRate * amount, 2);
        }

        public decimal GetFxRate(string fromCurrency, string toCurrency, DateTime date, string fixerIoApiKey)
        {
            if (fromCurrency == toCurrency)
            {
                return 1.0M;
            }

            var key = GetKey(fromCurrency, toCurrency, date);
            if (fxRateCache.ContainsKey(key) == false)
            {
                // Base Currency ist EUR
                var euroToFrom = GetFx(GetUrl(fromCurrency, date, fixerIoApiKey), fromCurrency);
                var euroToTo = GetFx(GetUrl(toCurrency, date, fixerIoApiKey), toCurrency);

                fxRateCache[key] = euroToTo / euroToFrom;
            }

            return fxRateCache[key];
        }

        private string GetUrl(string currency, DateTime date, string fixerIoApiKey)
        {
            var url = $"http://data.fixer.io/api/{date:yyyy-MM-dd}?access_key={fixerIoApiKey}&symbols={currency}";
            return url;
        }

        private decimal GetFx(string url, string currency)
        {
            var response = HttpGet(url);
            dynamic stuff = JObject.Parse(response);
            return stuff.rates[currency];
        }

        private string GetKey(string fromCurrency, string toCurrency, DateTime date)
        {
            return $"{fromCurrency}_{toCurrency}_{date:yyyyMMdd}";
        }

        private string HttpGet(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = request.GetResponseAsync().Result;
            string myResponse = "";
            using (var sr = new System.IO.StreamReader(response.GetResponseStream()))
            {
                myResponse = sr.ReadToEnd();
                return myResponse;
            }
        }
    }
}


