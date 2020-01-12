using System;
using System.IO;
using System.Text.RegularExpressions;
using Nancy;
using Nancy.Responses;
using NzbDrone.Core.MediaCover;

namespace Sonarr.Http.Frontend.Mappers
{
    public class MediaCoverProxyMapper : IMapHttpRequestsToDisk
    {
        private readonly Regex _regex = new Regex(@"/MediaCoverProxy/(?<hash>\w+)/(?<filename>(.+)\.(jpg|png|gif))");

        private readonly IMediaCoverProxy _mediaCoverProxy;

        public MediaCoverProxyMapper(IMediaCoverProxy mediaCoverProxy)
        {
            _mediaCoverProxy = mediaCoverProxy;
        }

        public string Map(string resourceUrl)
        {
            return null;
        }

        public bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/MediaCoverProxy/", StringComparison.InvariantCultureIgnoreCase);
        }

        public Response GetResponse(string resourceUrl)
        {
            var match = _regex.Match(resourceUrl);

            if (!match.Success)
            {
                return new NotFoundResponse();
            }

            var hash = match.Groups["hash"].Value;
            var filename = match.Groups["filename"].Value;

            var imageData = _mediaCoverProxy.GetImage(hash);

            return new StreamResponse(() => new MemoryStream(imageData), MimeTypes.GetMimeType(filename));
        }
    }
}
