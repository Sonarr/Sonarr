using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.SeriesStats;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Series
{
    [V3ApiController("series/lookup")]
    public class SeriesLookupController : Controller
    {
        private readonly ISearchForNewSeries _searchProxy;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IMapCoversToLocal _coverMapper;

        public SeriesLookupController(ISearchForNewSeries searchProxy, IBuildFileNames fileNameBuilder, IMapCoversToLocal coverMapper)
        {
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _coverMapper = coverMapper;
        }

        [HttpGet]
        public IEnumerable<SeriesResource> Search([FromQuery] string term, [FromQuery] int? language = null)
        {
            var languageId = language ?? Language.English.Id;
            var searchLanguage = Language.All.FirstOrDefault(l => l.Id == languageId);

            if (searchLanguage == null || IsoLanguages.Get(searchLanguage) == null)
            {
                throw new BadRequestException($"Invalid language: {languageId}");
            }

            var tvDbResults = _searchProxy.SearchForNewSeries(term, searchLanguage);
            return MapToResource(tvDbResults);
        }

        private IEnumerable<SeriesResource> MapToResource(IEnumerable<NzbDrone.Core.Tv.Series> series)
        {
            foreach (var currentSeries in series)
            {
                var resource = currentSeries.ToResource();

                _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images, resource.Added);

                var poster = currentSeries.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);

                if (poster != null)
                {
                    resource.RemotePoster = poster.RemoteUrl;
                }

                resource.Folder = _fileNameBuilder.GetSeriesFolder(currentSeries);
                resource.Folders = new List<SeriesFolderResource>();
                resource.Statistics = new SeriesStatistics().ToResource(resource.Seasons);
                resource.Translations = currentSeries.Translations.ToResource();

                foreach (var translation in currentSeries.Translations)
                {
                    resource.Folders.Add(new SeriesFolderResource
                    {
                        Language = translation.Language,
                        Folder = _fileNameBuilder.GetSeriesFolder(currentSeries, translation.Title)
                    });
                }

                yield return resource;
            }
        }
    }
}
