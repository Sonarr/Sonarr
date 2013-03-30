using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data.QGen.Dialects
{
    public class FirebirdDialect : Dialect
    {
        public override string CreateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            return token.Replace('[', new Char()).Replace(']', new Char());
        }
    }
}
