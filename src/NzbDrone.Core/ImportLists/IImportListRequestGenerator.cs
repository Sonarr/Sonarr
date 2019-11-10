namespace NzbDrone.Core.ImportLists
{
    public interface IImportListRequestGenerator
    {
        ImportListPageableRequestChain GetListItems();
    }
}
