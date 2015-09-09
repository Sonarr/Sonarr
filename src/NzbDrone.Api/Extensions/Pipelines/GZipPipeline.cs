using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using NLog;

namespace NzbDrone.Api.Extensions.Pipelines
{
    public class GzipCompressionPipeline : IRegisterNancyPipeline
    {
        private readonly Logger _logger;

        public GzipCompressionPipeline(Logger logger)
        {
            _logger = logger;
        }

        public void Register(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(c => CompressResponse(c.Request, c.Response));
        }

        private Response CompressResponse(Request request, Response response)
        {
            try
            {
                if (
                   !response.ContentType.Contains("image")
                && !response.ContentType.Contains("font")
                && request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"))
                && (!response.Headers.ContainsKey("Content-Encoding") || response.Headers["Content-Encoding"] != "gzip"))
                {
                    var data = new MemoryStream();
                    response.Contents.Invoke(data);
                    data.Position = 0;
                    if (data.Length < 1024)
                    {
                        response.Contents = stream =>
                        {
                            data.CopyTo(stream);
                            stream.Flush();
                        };
                    }
                    else
                    {
                        response.Headers["Content-Encoding"] = "gzip";
                        response.Contents = s =>
                        {
                            var gzip = new GZipStream(s, CompressionMode.Compress, true);
                            data.CopyTo(gzip);
                            gzip.Close();
                        };
                    }
                }

                return response;
            }

            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to gzip response");
                throw;
            }
        }
    }
}