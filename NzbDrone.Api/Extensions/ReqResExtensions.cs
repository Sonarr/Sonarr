using System;
using System.Collections.Generic;
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
            return (T)FromJson(body, type);
        }


        public static object FromJson(this Stream body, Type type)
        {
            var reader = new StreamReader(body, true);
            body.Position = 0;
            var value = reader.ReadToEnd();
            return Json.Deserialize(value, type);
        }

        public static JsonResponse<TModel> AsResponse<TModel>(this TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new JsonResponse<TModel>(model, NancySerializer) { StatusCode = statusCode };
        }

        public static IDictionary<string, string> DisableCache(this IDictionary<string, string> headers)
        {
            headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            headers.Add("Pragma", "no-cache");
            headers.Add("Expires", "0");

            return headers;
        }

        public static IDictionary<string, string> EnableCache(this IDictionary<string, string> headers)
        {
            headers.Add("Cache-Control", "max-age=31536000 , public");
            headers.Add("Expires", "Sat, 29 Jun 2020 00:00:00 GMT");
            headers.Add("Last-Modified", "Sat, 29 Jun 2000 00:00:00 GMT");
            headers.Add("Age", "193266");

            return headers;
        }
    }
}