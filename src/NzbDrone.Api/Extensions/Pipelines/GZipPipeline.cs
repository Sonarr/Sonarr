using System.IO;
using System.IO.Compression;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;

namespace NzbDrone.Api.Extensions.Pipelines
{
    public class GzipCompressionPipeline : IRegisterNancyPipeline
    {
        public void Register(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(c => CompressResponse(c.Request, c.Response));
        }

        private Response CompressResponse(Request request, Response response)
        {
            if (!response.ContentType.Contains("image")
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
    }
}