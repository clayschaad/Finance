using System;
using System.Collections.Generic;

namespace Schaad.Finance.Api
{
    public interface IPdfParsingService
    {
		int GetTotalPages(string file);
        IReadOnlyList<string> ExtractText(string file, int page);
    }
}