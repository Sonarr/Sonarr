using System;

namespace NzbDrone.Common.Expansive
{
    internal class PatternStyle
    {
        public string TokenMatchPattern { get; set; }
        public Func<string, string> TokenFilter { get; set; }
        public Func<string, string> TokenReplaceFilter { get; set; }
        public Func<string, string> OutputFilter { get; set; }
    }
}
