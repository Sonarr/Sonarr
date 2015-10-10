using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Api.Parse
{
    public class ParseModule : NzbDroneRestModule<ParseResource>
    {
        private readonly IParsingService _parsingService;

        public ParseModule(IParsingService parsingService)
        {
            _parsingService = parsingService;

            GetResourceSingle = Parse;
        }

        private ParseResource Parse()
        {
            var title = Request.Query.Title.Value;
            var parsedEpisodeInfo = Parser.ParseTitle(title);

            if (parsedEpisodeInfo == null)
            {
                return null;
            }

            var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0, 0);

            if (remoteEpisode == null)
            {
                remoteEpisode = new RemoteEpisode
                                {
                                    ParsedEpisodeInfo = parsedEpisodeInfo
                                };

                return new ParseResource
                       {
                           Title = title,
                           ParsedEpisodeInfo = parsedEpisodeInfo
                       };
            }

            var resource = ToResource(remoteEpisode);
            resource.Title = title;

            return resource;
        }
    }
}