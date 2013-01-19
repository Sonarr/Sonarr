using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;

namespace NzbDrone.Api.QualityType
{
    public static class JsonExtensions
    {
        public static T FromJson<T>(this Stream body)
        {
            var reader = new StreamReader(body, true);
            body.Position = 0;
            var value = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }

        public static Response AsResponse<TModel>(this TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            ISerializer serializer = new DefaultJsonSerializer();
            var jsonResponse = new JsonResponse<TModel>(model, serializer) {StatusCode = statusCode};
            return jsonResponse;
        }
    }
}