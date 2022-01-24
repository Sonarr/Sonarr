using System.Text.RegularExpressions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.CustomFormats
{
    public abstract class RegexSpecificationBase : CustomFormatSpecificationBase
    {
        protected Regex _regex;
        protected string _raw;

        [FieldDefinition(1, Label = "Regular Expression")]
        public string Value
        {
            get => _raw;
            set
            {
                _raw = value;
                _regex = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        protected bool MatchString(string compared)
        {
            if (compared == null || _regex == null)
            {
                return false;
            }

            return _regex.IsMatch(compared);
        }
    }
}
