using System.Net;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;
using RestSharp;

namespace NzbDrone.Core.Rest
{
    public static class RestSharpExtensions
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        public static IRestResponse ValidateResponse(this IRestResponse response, IRestClient restClient)
        {
            Ensure.That(() => response).IsNotNull();

            if (response.Request == null && response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            Ensure.That(() => response.Request).IsNotNull();
            Ensure.That(() => restClient).IsNotNull();

            Logger.Trace("Validating Responses from [{0}] [{1}] status: [{2}]", response.Request.Method, restClient.BuildUri(response.Request), response.StatusCode);

            if (response.ResponseUri == null)
            {
                Logger.ErrorException("Error communicating with server", response.ErrorException);
                throw response.ErrorException;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
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
    }
}