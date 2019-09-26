using Moq;
using NUnit.Framework;
using Schaad.Finance.Api;
using Schaad.Finance.Services;
using System.Collections.Generic;
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
            var pdfParsingServiceMock = new Mock<IPdfParsingService>();
            pdfParsingServiceMock.Setup(t => t.ExtractText(It.IsAny<string>(), It.IsAny<int>())).Returns(() => new List<string>()
            {
                "     Einkaufs-Datum",
                "     13.07.2019          14.07.2019          Clay Schaad          13.70",
                "     13.07.2019          14.07.2019          Clay Schaad2         23.50",
                "     dummy test",
                "     Saldo per    "
            });
            service = new CreditCardStatementService(pdfParsingServiceMock.Object);
        }

        [Test]
        public void ReadFile_CembraCsv()
        {
            var transactions = service.ReadFile(CreditCardProvider.CembraCsv, @"Data\cembra.csv", System.Text.Encoding.UTF8);

            Assert.That(transactions.Count, Is.EqualTo(4));
            Assert.That(transactions.Sum(t => t.Amount), Is.EqualTo(144.60m));
        }

        [Test]
        public void ReadFile_CembraPdf()
        {
            var transactions = service.ReadFile(CreditCardProvider.CembraPdf, "", System.Text.Encoding.UTF8);

            Assert.That(transactions.Count, Is.EqualTo(2));
            Assert.That(transactions.Sum(t => t.Amount), Is.EqualTo(37.20m));
        }

        //[Test]
        //public void ReadFile_CembraPdf2()
        //{
        //    var service2 = new CreditCardStatementService(new PdfParsingService());
        //    var transactions = service2.ReadFile(CreditCardProvider.CembraPdf, @"D:\Desktop\20190923_Cembra.pdf", System.Text.Encoding.UTF8);

        //    Assert.That(transactions.Count, Is.EqualTo(2));
        //    Assert.That(transactions.Sum(t => t.Amount), Is.EqualTo(37.20m));
        //}
    }
}
