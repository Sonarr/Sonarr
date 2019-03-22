﻿using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NLog;
using NzbDrone.Api;
using Sonarr.Http.REST;
using NzbDrone.Common.Serializer;
using RestSharp;
using System.Linq;
using Sonarr.Http;

namespace NzbDrone.Integration.Test.Client
{
    public class ClientBase
    {
        protected readonly IRestClient _restClient;
        protected readonly string _resource;
        protected readonly string _apiKey;
        protected readonly Logger _logger;

        public ClientBase(IRestClient restClient, string apiKey, string resource)
        {
            _restClient = restClient;
            _resource = resource;
            _apiKey = apiKey;

            _logger = LogManager.GetLogger("REST");
        }

        public RestRequest BuildRequest(string command = "")
        {
            var request = new RestRequest(_resource + "/" + command.Trim('/'))
            {
                RequestFormat = DataFormat.Json,
            };

            request.AddHeader("Authorization", _apiKey);
            request.AddHeader("X-Api-Key", _apiKey);

            return request;
        }

        public T Execute<T>(IRestRequest request, HttpStatusCode statusCode) where T : class, new()
        {
            _logger.Info("{0}: {1}", request.Method, _restClient.BuildUri(request));

            var response = _restClient.Execute(request);
            _logger.Info("Response: {0}", response.Content);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            AssertDisableCache(response.Headers);

            response.ErrorMessage.Should().BeNullOrWhiteSpace();

            response.StatusCode.Should().Be(statusCode);

            return Json.Deserialize<T>(response.Content);
        }

        private static void AssertDisableCache(IList<Parameter> headers)
        {
            headers.Single(c => c.Name == "Cache-Control").Value.Should().Be("no-cache, no-store, must-revalidate, max-age=0");
            headers.Single(c => c.Name == "Pragma").Value.Should().Be("no-cache");
            headers.Single(c => c.Name == "Expires").Value.Should().Be("0");
        }
    }

    public class ClientBase<TResource> : ClientBase
        where TResource : RestResource, new()
    {
        public ClientBase(IRestClient restClient, string apiKey, string resource = null)
            : base(restClient, apiKey, resource ?? new TResource().ResourceName)
        {

        }

        public List<TResource> All()
        {
            var request = BuildRequest();
            return Get<List<TResource>>(request);
        }

        public PagingResource<TResource> GetPaged(int pageNumber, int pageSize, string sortKey, string sortDir, string filterKey = null, string filterValue = null)
        {
            var request = BuildRequest();
            request.AddParameter("page", pageNumber);
            request.AddParameter("pageSize", pageSize);
            request.AddParameter("sortKey", sortKey);
            request.AddParameter("sortDir", sortDir);

            if (filterKey != null && filterValue != null)
            {
                request.AddParameter("filterKey", filterKey);
                request.AddParameter("filterValue", filterValue);
            }

            return Get<PagingResource<TResource>>(request);
        }

        public TResource Post(TResource body, HttpStatusCode statusCode = HttpStatusCode.Created)
        {
            var request = BuildRequest();
            request.AddBody(body);
            return Post<TResource>(request, statusCode);
        }

        public TResource Put(TResource body, HttpStatusCode statusCode = HttpStatusCode.Accepted)
        {
            var request = BuildRequest();
            request.AddBody(body);
            return Put<TResource>(request, statusCode);
        }

        public TResource Get(int id, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest(id.ToString());
            return Get<TResource>(request, statusCode);
        }

        public TResource GetSingle(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest();
            return Get<TResource>(request, statusCode);
        }

        public void Delete(int id)
        {
            var request = BuildRequest(id.ToString());
            Delete(request);
        }

        public object InvalidGet(int id, HttpStatusCode statusCode = HttpStatusCode.NotFound)
        {
            var request = BuildRequest(id.ToString());
            return Get<object>(request, statusCode);
        }

        public object InvalidPost(TResource body, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var request = BuildRequest();
            request.AddBody(body);
            return Post<object>(request, statusCode);
        }

        public object InvalidPut(TResource body, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var request = BuildRequest();
            request.AddBody(body);
            return Put<object>(request, statusCode);
        }

        public T Get<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.OK) where T : class, new()
        {
            request.Method = Method.GET;
            return Execute<T>(request, statusCode);
        }

        public T Post<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.Created) where T : class, new()
        {
            request.Method = Method.POST;
            return Execute<T>(request, statusCode);
        }

        public T Put<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.Accepted) where T : class, new()
        {
            request.Method = Method.PUT;
            return Execute<T>(request, statusCode);
        }

        public void Delete(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            request.Method = Method.DELETE;
            Execute<object>(request, statusCode);
        }
    }
}