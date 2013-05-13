using System;
using System.Collections.Generic;
using AutoMapper;
using FluentValidation;
using Nancy;
using NzbDrone.Api.Extensions;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Api.REST
{
    public abstract class RestModule<TResource> : NancyModule
        where TResource : RestResource, new()
    {
        private const string ROOT_ROUTE = "/";
        private const string ID_ROUTE = @"/(?<id>[\d]{1,7})";

        private Action<int> _deleteResource;
        private Func<int, TResource> _getResourceById;
        private Func<List<TResource>> _getResourceAll;
        private Func<PagingResource<TResource>, PagingResource<TResource>> _getResourcePaged;
        private Func<TResource> _getResourceSingle;
        private Func<TResource, TResource> _createResource;
        private Func<TResource, TResource> _updateResource;

        protected ResourceValidator<TResource> PostValidator { get; private set; }
        protected ResourceValidator<TResource> PutValidator { get; private set; }
        protected ResourceValidator<TResource> SharedValidator { get; private set; }

        protected void ValidateId(int id)
        {
            if (id <= 0)
            {
                throw new BadRequestException(id + " is not a valid ID");
            }
        }

        protected RestModule(string modulePath)
            : base(modulePath)
        {

            PostValidator = new ResourceValidator<TResource>();
            PutValidator = new ResourceValidator<TResource>();
            SharedValidator = new ResourceValidator<TResource>();
        }

        protected Action<int> DeleteResource
        {
            private get { return _deleteResource; }
            set
            {
                _deleteResource = value;
                Delete[ID_ROUTE] = options =>
                {
                    ValidateId(options.Id);
                    DeleteResource((int)options.Id);
                    return new Response { StatusCode = HttpStatusCode.OK };
                };
            }
        }

        protected Func<int, TResource> GetResourceById
        {
            private get { return _getResourceById; }
            set
            {
                _getResourceById = value;
                Get[ID_ROUTE] = options =>
                    {
                        ValidateId(options.Id);
                        var resource = GetResourceById((int)options.Id);
                        return resource.AsResponse();
                    };
            }
        }

        protected Func<List<TResource>> GetResourceAll
        {
            private get { return _getResourceAll; }
            set
            {
                _getResourceAll = value;

                Get[ROOT_ROUTE] = options =>
                {
                    var resource = GetResourceAll();
                    return resource.AsResponse();
                };
            }
        }

        protected Func<PagingResource<TResource>, PagingResource<TResource>> GetResourcePaged
        {
            private get { return _getResourcePaged; }
            set
            {
                _getResourcePaged = value;

                Get[ROOT_ROUTE] = options =>
                {
                    var resource = GetResourcePaged(ReadPagingResourceFromRequest());
                    return resource.AsResponse();
                };
            }
        }

        protected Func<TResource> GetResourceSingle
        {
            private get { return _getResourceSingle; }
            set
            {
                _getResourceSingle = value;

                Get[ROOT_ROUTE] = options =>
                {
                    var resource = GetResourceSingle();
                    return resource.AsResponse();
                };
            }
        }

        protected Func<TResource, TResource> CreateResource
        {
            private get { return _createResource; }
            set
            {
                _createResource = value;
                Post[ROOT_ROUTE] = options =>
                {
                    var resource = CreateResource(ReadFromRequest());
                    return resource.AsResponse(HttpStatusCode.Created);
                };

            }
        }

        protected Func<TResource, TResource> UpdateResource
        {
            private get { return _updateResource; }
            set
            {
                _updateResource = value;
                Put[ROOT_ROUTE] = options =>
                {
                    var resource = UpdateResource(ReadFromRequest());
                    return resource.AsResponse(HttpStatusCode.Accepted);
                };
            }
        }

        private TResource ReadFromRequest()
        {
            //TODO: handle when request is null
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

        private PagingResource<TResource> ReadPagingResourceFromRequest()
        {
            int pageSize;
            Int32.TryParse(PrimitiveExtensions.ToNullSafeString(Request.Query.PageSize), out pageSize);
            if (pageSize == 0) pageSize = 10;
            
            int page;
            Int32.TryParse(PrimitiveExtensions.ToNullSafeString(Request.Query.Page), out page);
            if (page == 0) page = 1;

            var sortKey = PrimitiveExtensions.ToNullSafeString(Request.Query.SortKey);
            if (String.IsNullOrEmpty(sortKey)) sortKey = "AirDate";

            var sortDirection = PrimitiveExtensions.ToNullSafeString(Request.Query.SortDir)
                                                   .Equals("Asc", StringComparison.InvariantCultureIgnoreCase)
                                                   ? SortDirection.Ascending
                                                   : SortDirection.Descending;

            var pagingResource = new PagingResource<TResource>
                                     {
                                         PageSize = pageSize,
                                         Page = page,
                                         SortKey = sortKey,
                                         SortDirection = sortDirection
                                     };

            return pagingResource;
        }
    }
}