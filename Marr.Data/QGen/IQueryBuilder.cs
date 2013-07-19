namespace Marr.Data.QGen
{
    public interface IQueryBuilder
    {
        string BuildQuery();
    }

    public interface ISortQueryBuilder : IQueryBuilder
    {
        string BuildQuery(bool useAltNames);
    }
}
