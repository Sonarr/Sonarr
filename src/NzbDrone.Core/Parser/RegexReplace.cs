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
        private readonly string _replacementFormat;
        private readonly MatchEvaluator _replacementFunc;

        public RegexReplace(string pattern, string replacement, RegexOptions regexOptions)
        {
            _regex = new Regex(pattern, regexOptions);
            _replacementFormat = replacement;
        }

        public RegexReplace(string pattern, MatchEvaluator replacement, RegexOptions regexOptions)
        {
            _regex = new Regex(pattern, regexOptions);
            _replacementFunc = replacement;
        }

        public string Replace(string input)
        {
            if (_replacementFunc != null)
            {
                return _regex.Replace(input, _replacementFunc);
            }
            else
            {
                return _regex.Replace(input, _replacementFormat);
            }
        }

        public bool TryReplace(ref string input)
        {
            var result = _regex.IsMatch(input);
            if (_replacementFunc != null)
            {
                input = _regex.Replace(input, _replacementFunc);
            }
            else
            {
                input = _regex.Replace(input, _replacementFormat);
            }

            return result;
        }
    }
}
