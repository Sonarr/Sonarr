namespace Workarr.Notifications.Trakt
{
    public static class TraktInterlacedTypes
    {
        public static readonly HashSet<string> InterlacedTypes = new (StringComparer.OrdinalIgnoreCase)
        {
            "Interlaced", "MBAFF", "PAFF"
        };
    }
}
