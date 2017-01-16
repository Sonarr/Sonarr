using System.Net;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;
using RestSharp;

namespace NzbDrone.Core.Rest
{
    public static class RestSharpExtensions
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(RestSharpExtensions));

        public static IRestResponse ValidateResponse(this IRestResponse response, IRestClient restClient)
        {
            Ensure.That(response, () => response).IsNotNull();

            if (response.Request == null && response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            Ensure.That(response.Request, () => response.Request).IsNotNull();
            Ensure.That(restClient, () => restClient).IsNotNull();

            Logger.Debug("Validating Responses from [{0}] [{1}] status: [{2}]", response.Request.Method, restClient.BuildUri(response.Request), response.StatusCode);

            if (response.ResponseUri == null)
            {
                Logger.Error(response.ErrorException, "Error communicating with server");
                throw response.ErrorException;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    {
                        return response;
                    }
                case HttpStatusCode.NoContent:
                    {
                        return response;
                    }
                case HttpStatusCode.Created:
                    {
                        return response;
                    }
                default:
                    {
                        Logger.Warn("[{0}] [{1}] Failed. [{2}]", response.Request.Method, response.ResponseUri.ToString(), response.StatusCode);
                        throw new RestException(response, restClient);
                    }
            }
        }

        public static T Read<T>(this IRestResponse restResponse, IRestClient restClient) where T : class, new()
        {
            restResponse.ValidateResponse(restClient);

            if (restResponse.Content != null)
            {
                Logger.Trace("Response: " + restResponse.Content);
            }

            return Json.Deserialize<T>(restResponse.Content);
        }

        public static T ExecuteAndValidate<T>(this IRestClient client, IRestRequest request) where T : class, new()
        {
            return client.Execute(request).Read<T>(client);
        }

        public static IRestResponse ExecuteAndValidate(this IRestClient client, IRestRequest request)
        {
            return client.Execute(request).ValidateResponse(client);
        }

        public static void AddQueryString(this IRestRequest request, string name, object value)
        {
            request.AddParameter(name, value.ToString(), ParameterType.GetOrPost);
        }

        public static object GetHeaderValue(this IRestResponse response, string key)
        {
            var header = response.Headers.FirstOrDefault(v => v.Name == key);

            if (header == null) return null;

            return header.Value;
        }
    }
}