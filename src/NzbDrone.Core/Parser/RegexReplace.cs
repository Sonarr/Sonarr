using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NzbDrone.Core.Parser
{
    public class RegexReplace
    {
        private readonly Regex _regex;
        private readonly string _replacement;

        public RegexReplace(string pattern, string replacement, RegexOptions regexOptions)
        {
            _regex = new Regex(pattern, regexOptions);
            _replacement = replacement;
        }

        public string Replace(string input)
        {
            return _regex.Replace(input, _replacement);
        }
    }
}
