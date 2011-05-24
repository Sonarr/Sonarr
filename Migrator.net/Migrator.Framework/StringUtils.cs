using System.Text;
using System.Text.RegularExpressions;

namespace Migrator.Framework
{
    public class StringUtils
    {
        /// <summary>
        /// Convert a classname to something more readable.
        /// ex.: CreateATable => Create a table
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string ToHumanName(string className)
        {
            string name = Regex.Replace(className, "([A-Z])", " $1").Substring(1);
            return name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="placeholder"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceOnce(string template, string placeholder, string replacement)
        {
            int loc = template.IndexOf(placeholder);
            if (loc < 0)
            {
                return template;
            }
            else
            {
                return new StringBuilder(template.Substring(0, loc))
                    .Append(replacement)
                    .Append(template.Substring(loc + placeholder.Length))
                    .ToString();
            }
        }
    }
}
