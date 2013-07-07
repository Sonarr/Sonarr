using System.IO;
using System.IO.Compression;
using System.Linq;
using Nancy;

namespace NzbDrone.Api.Extensions
{
    public static class GzipCompressionPipeline
    {
        public static void Handle(NancyContext context)
        {
            context.Response.CompressResponse(context.Request);
        }

        public static Response CompressResponse(this Response response, Request request)
        {
            if (!response.ContentType.Contains("image") && request.Headers.AcceptEncoding.Any(x => x.Contains("gzip")))
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