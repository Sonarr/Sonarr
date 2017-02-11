using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using NLog;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Extensions.Pipelines
{
    public class GzipCompressionPipeline : IRegisterNancyPipeline
    {
        private readonly Logger _logger;

        public int Order => 0;

        public GzipCompressionPipeline(Logger logger)
        {
            _logger = logger;
        }

        public void Register(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(CompressResponse);
        }

        private void CompressResponse(NancyContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (
                   response.Contents != Response.NoBody
                && !response.ContentType.Contains("image")
                && !response.ContentType.Contains("font")
                && request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"))
                && !AlreadyGzipEncoded(response)
                && !ContentLengthIsTooSmall(response))
                {
                    var contents = response.Contents;

                    response.Headers["Content-Encoding"] = "gzip";
                    response.Contents = responseStream =>
                    {
                        using (var gzip = new GZipStream(responseStream, CompressionMode.Compress, true))
                        using (var buffered = new BufferedStream(gzip, 8192))
                        {
                            contents.Invoke(buffered);
                        }
                    };
                }
            }

            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to gzip response");
                throw;
            }
        }

        private static bool ContentLengthIsTooSmall(Response response)
        {
            var contentLength = response.Headers.GetValueOrDefault("Content-Length");

            if (contentLength != null && long.Parse(contentLength) < 1024)
            {
                return true;
            }

            return false;
        }

        private static bool AlreadyGzipEncoded(Response response)
        {
            var contentEncoding = response.Headers.GetValueOrDefault("Content-Encoding");

            if (contentEncoding == "gzip")
            {
                return true;
            }

            return false;
        }
    }
}
