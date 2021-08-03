using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Extensions.Pipelines
{
    public class GzipCompressionPipeline : IRegisterNancyPipeline
    {
        private readonly Logger _logger;

        public int Order => 0;

        private readonly Action<Action<Stream>, Stream> _writeGZipStream;

        public GzipCompressionPipeline(Logger logger)
        {
            _logger = logger;

            // On Mono GZipStream/DeflateStream leaks memory if an exception is thrown, use an intermediate buffer in that case.
            _writeGZipStream = (Action<Action<Stream>, Stream>)WriteGZipStream;
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
                    response.Contents = responseStream => _writeGZipStream(contents, responseStream);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to gzip response");
                throw;
            }
        }

        private static void WriteGZipStreamMono(Action<Stream> innerContent, Stream targetStream)
        {
            using (var membuffer = new MemoryStream())
            {
                WriteGZipStream(innerContent, membuffer);
                membuffer.Position = 0;
                membuffer.CopyTo(targetStream);
            }
        }

        private static void WriteGZipStream(Action<Stream> innerContent, Stream targetStream)
        {
            using (var gzip = new GZipStream(targetStream, CompressionMode.Compress, true))
            using (var buffered = new BufferedStream(gzip, 8192))
            {
                innerContent.Invoke(buffered);
            }
        }

        private static bool ContentLengthIsTooSmall(Response response)
        {
            var contentLength = response.Headers.TryGetValue("Content-Length", out var value) ? value : null;

            if (contentLength != null && long.Parse(contentLength) < 1024)
            {
                return true;
            }

            return false;
        }

        private static bool AlreadyGzipEncoded(Response response)
        {
            var contentEncoding = response.Headers.TryGetValue("Content-Encoding", out var value) ? value : null;

            if (contentEncoding == "gzip")
            {
                return true;
            }

            return false;
        }
    }
}
