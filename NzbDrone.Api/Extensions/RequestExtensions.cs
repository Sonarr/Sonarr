using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;

namespace NzbDrone.Api.Extensions
{
    public static class JsonExtensions
    {
        public static T FromJson<T>(this Stream body)
        {
            var reader = new StreamReader(body, true);
            body.Position = 0;
            var value = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(value, Serializer.Settings);
        }

        public static JsonResponse<TModel> AsResponse<TModel>(this TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var jsonResponse = new JsonResponse<TModel>(model, new NancyJsonSerializer()) { StatusCode = statusCode };
            return jsonResponse;
        }
    }
}