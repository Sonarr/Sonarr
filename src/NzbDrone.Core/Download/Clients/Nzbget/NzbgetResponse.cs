namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetResponse<T>
    {
        public string Version { get; set; }

        public T Result { get; set; }
    }
}
