using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using NzbDrone.Core.MediaCover;

namespace Sonarr.Http.Frontend.Mappers
{
    public class MediaCoverProxyMapper : IMapHttpRequestsToDisk
    {
        private readonly Regex _regex = new Regex(@"/MediaCoverProxy/(?<hash>\w+)/(?<filename>(.+)\.(jpg|png|gif))");

        private readonly IMediaCoverProxy _mediaCoverProxy;
        private readonly IContentTypeProvider _mimeTypeProvider;

        public MediaCoverProxyMapper(IMediaCoverProxy mediaCoverProxy)
        {
            _mediaCoverProxy = mediaCoverProxy;
            _mimeTypeProvider = new FileExtensionContentTypeProvider();
        }

        public string Map(string resourceUrl)
        {
            return null;
        }

        public bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/MediaCoverProxy/", StringComparison.InvariantCultureIgnoreCase);
        }

        public IActionResult GetResponse(string resourceUrl)
        {
            var match = _regex.Match(resourceUrl);

            if (!match.Success)
            {
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
            }

            var hash = match.Groups["hash"].Value;
            var filename = match.Groups["filename"].Value;

            var imageData = _mediaCoverProxy.GetImage(hash);

            if (!_mimeTypeProvider.TryGetContentType(filename, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return new FileContentResult(imageData, contentType);
        }
    }
}
