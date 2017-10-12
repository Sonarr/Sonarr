using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Quality
{
    public class QualityProfileSchemaModule : SonarrRestModule<QualityProfileResource>
    {
        public QualityProfileSchemaModule()
            : base("/qualityprofile/schema")
        {
            GetResourceSingle = GetSchema;
        }

        private QualityProfileResource GetSchema()
        {
            var groupedQualites = NzbDrone.Core.Qualities.Quality.DefaultQualityDefinitions.GroupBy(q => q.Weight);
            var items = new List<ProfileQualityItem>();
            var groupId = 1000;

            foreach (var group in groupedQualites)
            {
                if (group.Count() == 1)
                {
                    items.Add(new ProfileQualityItem { Quality = group.First().Quality, Allowed = false });
                    continue;
                }

                items.Add(new ProfileQualityItem
                          {
                              Id = groupId,
                              Name = group.First().GroupName,
                              Items = group.Select(g => new ProfileQualityItem
                                                        {
                                                            Quality = g.Quality,
                                                            Allowed = false
                                                        }).ToList(),
                              Allowed = false
                          });

                groupId++;
            }

            var qualityProfile = new Profile();
            qualityProfile.Cutoff = NzbDrone.Core.Qualities.Quality.Unknown.Id;
            qualityProfile.Items = items;

            return qualityProfile.ToResource();
        }
    }
}