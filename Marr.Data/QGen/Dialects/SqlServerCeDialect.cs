using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data.QGen.Dialects
{
    public class SqlServerCeDialect : Dialect
    {
        public override string IdentityQuery
        {
            get
            {
                return "SELECT @@IDENTITY;";
            }
        }

        public override bool SupportsBatchQueries
        {
            get
            {
                return false;
            }
        }
    }
}
