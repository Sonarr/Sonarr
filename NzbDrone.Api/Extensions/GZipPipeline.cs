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
            if (!context.Response.ContentType.Contains("image") && context.Request.Headers.AcceptEncoding.Any(x => x.Contains("gzip")))
            {
                var data = new MemoryStream();
                context.Response.Contents.Invoke(data);
                data.Position = 0;
                if (data.Length < 1024)
                {
                    context.Response.Contents = stream =>
                    {
                        data.CopyTo(stream);
                        stream.Flush();
                    };
                }
                else
                {
                    context.Response.Headers["Content-Encoding"] = "gzip";
                    context.Response.Contents = s =>
                    {
                        var gzip = new GZipStream(s, CompressionMode.Compress, true);
                        data.CopyTo(gzip);
                        gzip.Close();
                    };
                }
            }
        }
    }
}