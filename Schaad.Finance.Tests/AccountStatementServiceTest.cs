using NUnit.Framework;
using Schaad.Finance.Services;

namespace Schaad.Finance.Tests
{
    [TestFixture]
    public class AccountStatementServiceTest
    {
        private AccountStatementService service;

        [SetUp]
        public void Startup()
        {
            service = new AccountStatementService();
        }

        [Test]
        public void ReadFile_MT940()
        {
            var statements = service.ReadFile(@"Data\mt940.sta", System.Text.Encoding.UTF8);

            Assert.That(statements.TrueForAll(s => s.IsSuccess));
        }

        [Test]
        public void ReadFile_CAMT053()
        {
            //var statements = service.ReadFile(@"Data\camt053.xml", System.Text.Encoding.UTF8);
            var statements = service.ReadFile(@"C:\temp\camt053.xml", System.Text.Encoding.UTF8);

            Assert.That(statements.TrueForAll(s => s.IsSuccess));
        }
    }
}
