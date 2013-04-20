using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NzbDrone.Api.Extensions;
using System.Linq;

namespace NzbDrone.Api.REST
{
    public abstract class RestModule<TResource> : NancyModule
        where TResource : RestResource, new()
    {
        protected ResourceValidator<TResource> PostValidator { get; private set; }
        protected ResourceValidator<TResource> PutValidator { get; private set; }
        protected ResourceValidator<TResource> SharedValidator { get; private set; }
        private const string ROOT_ROUTE = "/";
        private const string ID_ROUTE = "/{id}";

        protected RestModule()
            : this(new TResource().ResourceName)
        {

        }

        protected RestModule(string modulePath)
            : base(modulePath)
        {

            PostValidator = new ResourceValidator<TResource>();
            PutValidator = new ResourceValidator<TResource>();
            SharedValidator = new ResourceValidator<TResource>();

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

            var errors = SharedValidator.Validate(resource).Errors.ToList();


            if (Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddRange(PostValidator.Validate(resource).Errors);
            }
            else if (Request.Method.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddRange(PutValidator.Validate(resource).Errors);
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }

            return resource;
        }
    }
}