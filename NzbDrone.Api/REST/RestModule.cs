using System;
using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.REST
{
    public abstract class RestModule<TResource> : NancyModule
        where TResource : RestResource<TResource>, new()
    {
        private const string ROOT_ROUTE = "/";
        private const string ID_ROUTE = "/{id}";

        protected RestModule()
            : this(new TResource().ResourceName)
        {

        }

        protected RestModule(string modulePath)
            : base(modulePath)
        {
            Get[ROOT_ROUTE] = options =>
            {
                EnsureImplementation(GetResourceAll);
                var resource = GetResourceAll();
                return resource.AsResponse();
            };

            Get[ID_ROUTE] = options =>
            {
                EnsureImplementation(GetResourceById);
                var resource = GetResourceById(options.Id);
                return resource.AsResponse();
            };

            Post[ROOT_ROUTE] = options =>
            {
                EnsureImplementation(CreateResource);
                var resource = CreateResource(ReadFromRequest());
                return resource.AsResponse(HttpStatusCode.Created);
            };

            Put[ROOT_ROUTE] = options =>
            {
                EnsureImplementation(UpdateResource);
                var resource = UpdateResource(ReadFromRequest());
                return resource.AsResponse(HttpStatusCode.Accepted);
            };

            Delete[ID_ROUTE] = options =>
            {
                EnsureImplementation(DeleteResource);
                DeleteResource(options.Id);
                return new Response { StatusCode = HttpStatusCode.OK };
            };
        }

        protected Action<int> DeleteResource { get; set; }
        protected Func<int, TResource> GetResourceById { get; set; }
        protected Func<List<TResource>> GetResourceAll { get; set; }
        protected Func<TResource, TResource> CreateResource { get; set; }
        protected Func<TResource, TResource> UpdateResource { get; set; }

        private void EnsureImplementation(Delegate implementation)
        {
            if (implementation == null)
            {
                throw new NotImplementedException();
            }
        }

        private TResource ReadFromRequest()
        {
            var resource = Request.Body.FromJson<TResource>();

            if (Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                resource.ValidateForPost();
            }
            else if (Request.Method.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
            {
                resource.ValidateForPut();
            }

            return resource;
        }
    }
}