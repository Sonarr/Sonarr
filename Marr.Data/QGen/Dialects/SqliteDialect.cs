using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data.QGen.Dialects
{
    public class SqliteDialect : Dialect
    {
        public override string IdentityQuery
        {
            get
            {
                return "SELECT last_insert_rowid();";
            }
        }
    }
}
