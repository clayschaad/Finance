using NUnit.Framework;
using Schaad.Finance.Services;

namespace Schaad.Finance.Tests
{
    [TestFixture]
    public class FxServiceTest
    {
        private FxService service;

        private const string FIXERIO_API_KEY = "<insert fixer io api key here>";

        [SetUp]
        public void Startup()
        {
            service = new FxService();
        }

        [Test]
        public void GetFxRate_EUR2CHF()
        {
            var fxRate = service.GetFxRate("EUR", "CHF", new System.DateTime(2019, 07, 01), FIXERIO_API_KEY);

            Assert.That(fxRate, Is.EqualTo(1.114262m));
        }

        [Test]
        public void ConvertCurrency_100EUR2CHF()
        {
            var amount = service.ConvertCurrency(100, "EUR", "CHF", FIXERIO_API_KEY);

            Assert.That(amount, Is.GreaterThan(100));
        }
    }
}
