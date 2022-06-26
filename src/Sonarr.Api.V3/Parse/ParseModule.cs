﻿using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser;
using Sonarr.Api.V3.Episodes;
using Sonarr.Api.V3.Series;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Parse
{
    public class ParseModule : SonarrRestModule<ParseResource>
    {
        private readonly IParsingService _parsingService;

        public ParseModule(IParsingService parsingService)
        {
            _parsingService = parsingService;

            GetResourceSingle = Parse;
        }

        private ParseResource Parse()
        {
            var title = Request.Query.Title.Value as string;
            var path = Request.Query.Path.Value as string;

            if (path.IsNullOrWhiteSpace() && title.IsNullOrWhiteSpace())
            {
                throw new BadRequestException("title or path is missing");
            }

            var parsedEpisodeInfo = path.IsNotNullOrWhiteSpace() ? Parser.ParsePath(path) : Parser.ParseTitle(title);

            if (parsedEpisodeInfo == null)
            {
                return new ParseResource
                {
                    Title = title
                };
            }

            var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0, 0);

            if (remoteEpisode != null)
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedEpisodeInfo = remoteEpisode.ParsedEpisodeInfo,
                    Series = remoteEpisode.Series.ToResource(),
                    Episodes = remoteEpisode.Episodes.ToResource()
                };
            }
            else
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedEpisodeInfo = parsedEpisodeInfo
                };
            }
        }
    }
}
