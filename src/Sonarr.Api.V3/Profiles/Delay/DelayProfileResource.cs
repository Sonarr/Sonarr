using System.Collections.Generic;
using System.Linq;
using Sonarr.Http.REST;
using Workarr.Indexers;
using Workarr.Profiles.Delay;

namespace Sonarr.Api.V3.Profiles.Delay
{
    public class DelayProfileResource : RestResource
    {
        public bool EnableUsenet { get; set; }
        public bool EnableTorrent { get; set; }
        public DownloadProtocol PreferredProtocol { get; set; }
        public int UsenetDelay { get; set; }
        public int TorrentDelay { get; set; }
        public bool BypassIfHighestQuality { get; set; }
        public bool BypassIfAboveCustomFormatScore { get; set; }
        public int MinimumCustomFormatScore { get; set; }
        public int Order { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public static class DelayProfileResourceMapper
    {
        public static DelayProfileResource ToResource(this DelayProfile model)
        {
            if (model == null)
            {
                return null;
            }

            return new DelayProfileResource
            {
                Id = model.Id,

                EnableUsenet = model.EnableUsenet,
                EnableTorrent = model.EnableTorrent,
                PreferredProtocol = model.PreferredProtocol,
                UsenetDelay = model.UsenetDelay,
                TorrentDelay = model.TorrentDelay,
                BypassIfHighestQuality = model.BypassIfHighestQuality,
                BypassIfAboveCustomFormatScore = model.BypassIfAboveCustomFormatScore,
                MinimumCustomFormatScore = model.MinimumCustomFormatScore,
                Order = model.Order,
                Tags = new HashSet<int>(model.Tags)
            };
        }

        public static DelayProfile ToModel(this DelayProfileResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new DelayProfile
            {
                Id = resource.Id,

                EnableUsenet = resource.EnableUsenet,
                EnableTorrent = resource.EnableTorrent,
                PreferredProtocol = resource.PreferredProtocol,
                UsenetDelay = resource.UsenetDelay,
                TorrentDelay = resource.TorrentDelay,
                BypassIfHighestQuality = resource.BypassIfHighestQuality,
                BypassIfAboveCustomFormatScore = resource.BypassIfAboveCustomFormatScore,
                MinimumCustomFormatScore = resource.MinimumCustomFormatScore,
                Order = resource.Order,
                Tags = new HashSet<int>(resource.Tags)
            };
        }

        public static List<DelayProfileResource> ToResource(this IEnumerable<DelayProfile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
