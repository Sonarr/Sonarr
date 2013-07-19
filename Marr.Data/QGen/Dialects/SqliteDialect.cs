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

        public override string StartsWithFormat
        {
            get { return "({0} LIKE {1} || '%')"; }
        }

        public override string EndsWithFormat
        {
            get { return "({0} LIKE '%' || {1})"; }
        }

        public override string ContainsFormat
        {
            get { return "({0} LIKE '%' || {1} || '%')"; }
        }
    }
}
