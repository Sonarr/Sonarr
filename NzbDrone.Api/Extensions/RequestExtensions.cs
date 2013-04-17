using System.IO;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common;

namespace NzbDrone.Api.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();
        private static readonly NancyJsonSerializer NancySerializer = new NancyJsonSerializer(Serializer);

        public static T FromJson<T>(this Stream body) where T : class, new()
        {
            var reader = new StreamReader(body, true);
            body.Position = 0;
            var value = reader.ReadToEnd();
            return Serializer.Deserialize<T>(value);
        }

        public static JsonResponse<TModel> AsResponse<TModel>(this TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new JsonResponse<TModel>(model, NancySerializer) { StatusCode = statusCode };
        }
    }
}