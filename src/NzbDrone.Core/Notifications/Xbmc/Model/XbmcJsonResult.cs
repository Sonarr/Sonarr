namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class XbmcJsonResult<T>
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public T Result { get; set; }
    }
}
