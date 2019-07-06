using NUnit.Framework;
using Schaad.Finance.Api;
using Schaad.Finance.Services;
using System.Linq;

namespace Schaad.Finance.Tests
{
    [TestFixture]
    public class CreditCardStatementServiceTest
    {
        private CreditCardStatementService service;

        [SetUp]
        public void Startup()
        {
            service = new CreditCardStatementService();
        }

        [Test]
        public void ReadFile_Cembra()
        {
            var transactions = service.ReadFile(CreditCardProvider.Cembra, @"Data\cembra.csv", System.Text.Encoding.UTF8);

            Assert.That(transactions.Count, Is.EqualTo(4));
            Assert.That(transactions.Sum(t => t.Amount), Is.EqualTo(144.60m));
        }
    }
}
