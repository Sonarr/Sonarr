using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Sonarr
{
    public class SonarrImport : ImportListBase<SonarrSettings>
    {
        private readonly ISonarrV3Proxy _sonarrV3Proxy;
        public override string Name => "Sonarr";

        public override ImportListType ListType => ImportListType.Program;

        public SonarrImport(ISonarrV3Proxy sonarrV3Proxy,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            Logger logger)
            : base(importListStatusService, configService, parsingService, logger)
        {
            _sonarrV3Proxy = sonarrV3Proxy;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            var series = new List<ImportListItemInfo>();

            try
            {
                var remoteSeries = _sonarrV3Proxy.GetSeries(Settings);

                foreach (var item in remoteSeries)
                {
                    if (!Settings.ProfileIds.Any() || Settings.ProfileIds.Contains(item.QualityProfileId))
                    {
                        series.Add(new ImportListItemInfo
                        {
                            TvdbId = item.TvdbId,
                            Title = item.Title
                        });
                    }
                }

                _importListStatusService.RecordSuccess(Definition.Id);
            }
            catch
            {
                _importListStatusService.RecordFailure(Definition.Id);
            }

            return CleanupListItems(series);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "getDevices")
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

                var devices = _sonarrV3Proxy.GetProfiles(Settings);

                return new
                {
                    options = devices.OrderBy(d => d.Name, StringComparer.InvariantCultureIgnoreCase)
                                            .Select(d => new
                                            {
                                                id = d.Id,
                                                name = d.Name
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
