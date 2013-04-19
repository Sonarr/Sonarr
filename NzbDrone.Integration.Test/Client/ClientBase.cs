using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NLog;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public abstract class ClientBase<TResource> where TResource : new()
    {
        private readonly IRestClient _restClient;
        private readonly string _resource;

        private readonly Logger _logger;

        protected ClientBase(IRestClient restClient, string resource)
        {
            _restClient = restClient;
            _resource = resource;
            _logger = LogManager.GetLogger("REST");
        }

        public List<TResource> GetAll()
        {
            var request = BuildRequest();
            return Get<List<TResource>>(request);
        }

        public TResource Post(TResource body)
        {
            var request = BuildRequest();
            request.AddBody(body);
            return Post<TResource>(request);
        }

        protected RestRequest BuildRequest(string command = "")
        {
            return new RestRequest(_resource + "/" + command.Trim('/'))
                {
                    RequestFormat = DataFormat.Json
                };
        }

        protected T Get<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.OK) where T : new()
        {
            request.Method = Method.GET;
            return Execute<T>(request, statusCode);
        }

        public T Post<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.Created) where T : new()
        {
            request.Method = Method.POST;
            return Execute<T>(request, statusCode);
        }

        private T Execute<T>(IRestRequest request, HttpStatusCode statusCode) where T : new()
        {
            _logger.Info("{0}: {1}", request.Method, _restClient.BuildUri(request));

            var response = _restClient.Execute<T>(request);

            _logger.Info("Response: {0}", response.Content);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            response.ErrorMessage.Should().BeBlank();


            response.StatusCode.Should().Be(statusCode);

            return response.Data;
        }

    }
}