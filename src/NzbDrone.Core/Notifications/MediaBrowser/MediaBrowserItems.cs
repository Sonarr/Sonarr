using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowserItems
    {
        public List<MediaBrowserItem> Items { get; set; }
    }

    public class MediaBrowserItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public MediaBrowserProviderIds ProviderIds { get; set; }
    }

    public class MediaBrowserProviderIds
    {
        public string Imdb { get; set; }
        public int Tvdb { get; set; }
        public int TvMaze { get; set; }
        public int TvRage { get; set; }
    }

    public enum MediaBrowserMatchQuality
    {
        Id = 0,
        Name = 1,
        None = 2
    }
}
