namespace Workarr.Notifications.Xbmc.Model
{
    public class VersionResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public Dictionary<string, int> Result { get; set; }
    }
}
