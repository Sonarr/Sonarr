namespace Marr.Data.QGen.Dialects
{
    public class SqlServerDialect : Dialect
    {
        public override string IdentityQuery
        {
            get
            {
                return "SELECT SCOPE_IDENTITY();";
            }
        }
    }
}
