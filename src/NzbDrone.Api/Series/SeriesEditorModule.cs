using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Profiles.Languages;
using Sonarr.Http.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Series
{
    public class SeriesEditorModule : NzbDroneApiModule
    {
        private readonly ISeriesService _seriesService;
        private readonly ILanguageProfileService _languageProfileService;

        public SeriesEditorModule(ISeriesService seriesService, ILanguageProfileService languageProfileService)
            : base("/series/editor")
        {
            _seriesService = seriesService;
            _languageProfileService = languageProfileService;
            Put("/",  series => SaveAll());
        }

        private object SaveAll()
        {
            var resources = Request.Body.FromJson<List<SeriesResource>>();

            var seriesToUpdate = resources.Select(seriesResource =>
            {
                var series = _seriesService.GetSeries(seriesResource.Id);
                var updatedSeries = seriesResource.ToModel(series);

                // If the new language profile doens't exist, keep it the same.
                // This could happen if a 3rd-party app uses this endpoint to update a
                // series and doesn't pass the languageProfileI as well.

                if (!_languageProfileService.Exists(updatedSeries.LanguageProfileId))
                {
                    updatedSeries.LanguageProfileId = series.LanguageProfileId;
                }

                return updatedSeries;
            }).ToList();

            return ResponseWithCode(_seriesService.UpdateSeries(seriesToUpdate, true)
                                 .ToResource(false)
                                 , HttpStatusCode.Accepted);
        }
    }
}
