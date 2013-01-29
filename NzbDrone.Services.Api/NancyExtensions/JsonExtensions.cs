using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Responses;
using ServiceStack.Text;

namespace NzbDrone.Services.Api.NancyExtensions
{
    public static class JsonExtensions
    {
        public static T FromJson<T>(this Stream body)
        {
            var reader = new StreamReader(body, true);
            return JsonSerializer.DeserializeFromReader<T>(reader);
        }

        public static JsonResponse<TModel> AsResponse<TModel>(this TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var jsonResponse = new JsonResponse<TModel>(model, new ServiceStackSerializer()) { StatusCode = statusCode };
            return jsonResponse;
        }
    }
}