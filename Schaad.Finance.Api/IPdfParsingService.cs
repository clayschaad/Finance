using System;
using System.Collections.Generic;

namespace Schaad.Finance.Api
{
    public interface IPdfParsingService
    {
        IReadOnlyList<string> ExtractText(string file, int page);
    }
}