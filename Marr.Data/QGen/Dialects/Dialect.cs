using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data.QGen.Dialects
{
    public class Dialect
    {
        /// <summary>
        /// The default token is surrounded by brackets.
        /// </summary>
        /// <param name="token"></param>
        public virtual string CreateToken(string token)
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

                sb.Append("[").Append(part).Append("]");
            }

            return sb.ToString();
        }

        public virtual string IdentityQuery
        {
            get
            {
                return null;
            }
        }

        public bool HasIdentityQuery
        {
            get
            {
                return !string.IsNullOrEmpty(IdentityQuery);
            }
        }

        public virtual bool SupportsBatchQueries
        {
            get
            {
                return true;
            }
        }
    }
}
