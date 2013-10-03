using System;
using System.Linq;
using System.Text;

namespace Marr.Data.QGen.Dialects
{
    public class OracleDialect : Dialect
    {
        public override string CreateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            string[] parts = token.Replace('[', new Char()).Replace(']', new Char()).Split('.');

            StringBuilder sb = new StringBuilder();
            foreach (string part in parts)
            {
                if (sb.Length > 0)
                    sb.Append(".");

                bool hasSpaces = part.Contains(' ');

                if (hasSpaces)
                    sb.Append("[").Append(part).Append("]");
                else
                    sb.Append(part);
            }

            return sb.ToString();
        }
    }
}
