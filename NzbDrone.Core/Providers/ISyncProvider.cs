namespace NzbDrone.Core.Providers
{
    public interface ISyncProvider
    {
        void SyncUnmappedFolders();
        void BeginSyncUnmappedFolders();
    }
}