using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Sonarr
{
    public class SonarrImport : ImportListBase<SonarrSettings>
    {
        private readonly ISonarrV3Proxy _sonarrV3Proxy;
        public override string Name => "Sonarr";

        public override ImportListType ListType => ImportListType.Program;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromMinutes(5);

        public SonarrImport(ISonarrV3Proxy sonarrV3Proxy,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            ILocalizationService localizationService,
                            Logger logger)
            : base(importListStatusService, configService, parsingService, localizationService, logger)
        {
            _sonarrV3Proxy = sonarrV3Proxy;
        }

        public override ImportListFetchResult Fetch()
        {
            var series = new List<ImportListItemInfo>();
            var anyFailure = false;
            try
            {
                var remoteSeries = _sonarrV3Proxy.GetSeries(Settings);

                foreach (var item in remoteSeries)
                {
                    if (Settings.ProfileIds.Any() && !Settings.ProfileIds.Contains(item.QualityProfileId))
                    {
                        continue;
                    }

                    if (Settings.LanguageProfileIds.Any() && !Settings.LanguageProfileIds.Contains(item.LanguageProfileId))
                    {
                        continue;
                    }

                    if (Settings.TagIds.Any() && !Settings.TagIds.Any(tagId => item.Tags.Any(itemTagId => itemTagId == tagId)))
                    {
                        continue;
                    }

                    if (Settings.RootFolderPaths.Any() && !Settings.RootFolderPaths.Any(rootFolderPath => item.RootFolderPath.ContainsIgnoreCase(rootFolderPath)))
                    {
                        continue;
                    }

                    var info = new ImportListItemInfo
                    {
                        TvdbId = item.TvdbId,
                        Title = item.Title
                    };

                    if (Settings.SyncSeasonMonitoring)
                    {
                        info.Seasons = item.Seasons.Select(s => new Season
                        {
                            SeasonNumber = s.SeasonNumber,
                            Monitored = s.Monitored
                        }).ToList();
                    }

                    series.Add(info);
                }

                _importListStatusService.RecordSuccess(Definition.Id);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to fetch data for list {0} ({1})", Definition.Name, Name);

                _importListStatusService.RecordFailure(Definition.Id);
                anyFailure = true;
            }

            return new ImportListFetchResult(CleanupListItems(series), anyFailure);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            // Return early if there is not an API key
            if (Settings.ApiKey.IsNullOrWhiteSpace())
            {
                return new
                {
                    devices = new List<object>()
                };
            }

            Settings.Validate().Filter("ApiKey").ThrowOnError();

            if (action == "getProfiles")
            {
                var profiles = _sonarrV3Proxy.GetQualityProfiles(Settings);

                return new
                {
                    options = profiles.OrderBy(d => d.Name, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Id,
                            name = d.Name
                        })
                };
            }

            if (action == "getLanguageProfiles")
            {
                var langProfiles = _sonarrV3Proxy.GetLanguageProfiles(Settings);

                return new
                {
                    options = langProfiles.OrderBy(d => d.Name, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Id,
                            name = d.Name
                        })
                };
            }

            if (action == "getTags")
            {
                var tags = _sonarrV3Proxy.GetTags(Settings);

                return new
                {
                    options = tags.OrderBy(d => d.Label, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Id,
                            name = d.Label
                        })
                };
            }

            if (action == "getRootFolders")
            {
                var remoteRootFolders = _sonarrV3Proxy.GetRootFolders(Settings);

                return new
                {
                    options = remoteRootFolders.OrderBy(d => d.Path, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Path,
                            name = d.Path
                        })
                };
            }

            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_sonarrV3Proxy.Test(Settings));
        }
    }
}
