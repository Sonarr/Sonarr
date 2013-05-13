using System;
using System.IO;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Api.Extensions
{
    public static class JsonExtensions
    {
        private static readonly NancyJsonSerializer NancySerializer = new NancyJsonSerializer();

        public static T FromJson<T>(this Stream body) where T : class, new()
        {
            return FromJson<T>(body, typeof(T));
        }

        public static T FromJson<T>(this Stream body, Type type)
        {
            var reader = new StreamReader(body, true);
            body.Position = 0;
            var value = reader.ReadToEnd();
            return (T)Json.Deserialize(value, type);
        }

        public static JsonResponse<TModel> AsResponse<TModel>(this TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new JsonResponse<TModel>(model, NancySerializer) { StatusCode = statusCode };
        }
    }
}