using Schaad.Finance.Api;
using Spire.Pdf;
using System.Collections.Generic;
using System.Linq;

namespace Schaad.Finance.Services
{
    public class PdfParsingService : IPdfParsingService
    {
		public int GetTotalPages(string file)
        {
            var document = new PdfDocument();
            document.LoadFromFile(file);
            var pages = document.Pages.Count;
            return pages;
        }
		
        public IReadOnlyList<string> ExtractText(string file, int page)
        {
            var document = new PdfDocument();
            document.LoadFromFile(file);
            var text = document.Pages[page - 1].ExtractText();
            return text.Split('\n').ToList();
        }
    }
}
