using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class creates a SQL delete query.
    /// </summary>
    public class DeleteQuery : IQuery
    {
        protected Table TargetTable { get; set; }
        protected string WhereClause { get; set; }
        protected Dialect Dialect { get; set; }

        public DeleteQuery(Dialect dialect, Table targetTable, string whereClause)
        {
            Dialect = dialect;
            TargetTable = targetTable;
            WhereClause = whereClause;
        }

        public string Generate()
        {
            return string.Format("DELETE FROM {0} {1} ",
                Dialect.CreateToken(TargetTable.Name), 
                WhereClause);
        }
    }
}
