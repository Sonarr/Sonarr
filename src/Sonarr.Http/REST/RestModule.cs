using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Nancy;
using Nancy.Responses.Negotiation;
using NzbDrone.Core.Datastore;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.REST
{
    public abstract class RestModule<TResource> : NancyModule
        where TResource : RestResource, new()
    {
        private const string ROOT_ROUTE = "/";
        private const string ID_ROUTE = @"/(?<id>[\d]{1,10})";

        private HashSet<string> _excludedKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
                                                {
                                                    "page",
                                                    "pageSize",
                                                    "sortKey",
                                                    "sortDirection",
                                                    "filterKey",
                                                    "filterValue",
                                                };

        private Action<int> _deleteResource;
        private Func<int, TResource> _getResourceById;
        private Func<List<TResource>> _getResourceAll;
        private Func<PagingResource<TResource>, PagingResource<TResource>> _getResourcePaged;
        private Func<TResource> _getResourceSingle;
        private Func<TResource, int> _createResource;
        private Action<TResource> _updateResource;

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
            ValidateModule();

            PostValidator = new ResourceValidator<TResource>();
            PutValidator = new ResourceValidator<TResource>();
            SharedValidator = new ResourceValidator<TResource>();
        }

        private void ValidateModule()
        {
            if (GetResourceById != null)
            {
                return;
            }

            if (CreateResource != null || UpdateResource != null)
            {
                throw new InvalidOperationException("GetResourceById route must be defined before defining Create/Update routes.");
            }
        }

        protected Action<int> DeleteResource
        {
            private get
            {
                return _deleteResource;
            }

            set
            {
                _deleteResource = value;
                Delete(ID_ROUTE, options =>
                {
                    ValidateId(options.Id);
                    DeleteResource((int)options.Id);

                    return new object();
                });
            }
        }

        protected Func<int, TResource> GetResourceById
        {
            get
            {
                return _getResourceById;
            }

            set
            {
                _getResourceById = value;
                Get(ID_ROUTE, options =>
                    {
                        ValidateId(options.Id);
                        try
                        {
                            var resource = GetResourceById((int)options.Id);

                            if (resource == null)
                            {
                                return new NotFoundResponse();
                            }

                            return resource;
                        }
                        catch (ModelNotFoundException)
                        {
                            return new NotFoundResponse();
                        }
                    });
            }
        }

        protected Func<List<TResource>> GetResourceAll
        {
            private get
            {
                return _getResourceAll;
            }

            set
            {
                _getResourceAll = value;
                Get(ROOT_ROUTE, options =>
                {
                    var resource = GetResourceAll();
                    return resource;
                });
            }
        }

        protected Func<PagingResource<TResource>, PagingResource<TResource>> GetResourcePaged
        {
            private get
            {
                return _getResourcePaged;
            }

            set
            {
                _getResourcePaged = value;
                Get(ROOT_ROUTE, options =>
                {
                    var resource = GetResourcePaged(ReadPagingResourceFromRequest());
                    return resource;
                });
            }
        }

        protected Func<TResource> GetResourceSingle
        {
            private get
            {
                return _getResourceSingle;
            }

            set
            {
                _getResourceSingle = value;
                Get(ROOT_ROUTE, options =>
                {
                    var resource = GetResourceSingle();
                    return resource;
                });
            }
        }

        protected Func<TResource, int> CreateResource
        {
            private get
            {
                return _createResource;
            }

            set
            {
                _createResource = value;
                Post(ROOT_ROUTE, options =>
                {
                    var id = CreateResource(ReadResourceFromRequest());
                    return ResponseWithCode(GetResourceById(id), HttpStatusCode.Created);
                });
            }
        }

        protected Action<TResource> UpdateResource
        {
            private get
            {
                return _updateResource;
            }

            set
            {
                _updateResource = value;
                Put(ROOT_ROUTE, options =>
                    {
                        var resource = ReadResourceFromRequest();
                        UpdateResource(resource);
                        return ResponseWithCode(GetResourceById(resource.Id), HttpStatusCode.Accepted);
                    });
                Put(ID_ROUTE, options =>
                    {
                        var resource = ReadResourceFromRequest();
                        resource.Id = options.Id;
                        UpdateResource(resource);
                        return ResponseWithCode(GetResourceById(resource.Id), HttpStatusCode.Accepted);
                    });
            }
        }

        protected Negotiator ResponseWithCode(object model, HttpStatusCode statusCode)
        {
            return Negotiate.WithModel(model).WithStatusCode(statusCode);
        }

        protected TResource ReadResourceFromRequest(bool skipValidate = false, bool skipSharedValidate = false)
        {
            TResource resource;

            try
            {
                resource = Request.Body.FromJson<TResource>();
            }
            catch (JsonException e)
            {
                throw new BadRequestException($"Invalid request body. {e.Message}");
            }

            if (resource == null)
            {
                throw new BadRequestException("Request body can't be empty");
            }

            var errors = new List<ValidationFailure>();

            if (!skipSharedValidate)
            {
                errors.AddRange(SharedValidator.Validate(resource).Errors);
            }

            if (Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) && !skipValidate && !Request.Url.Path.EndsWith("/test", StringComparison.InvariantCultureIgnoreCase))
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
            int.TryParse(Request.Query.PageSize.ToString(), out pageSize);
            if (pageSize == 0)
            {
                pageSize = 10;
            }

            int page;
            int.TryParse(Request.Query.Page.ToString(), out page);
            if (page == 0)
            {
                page = 1;
            }

            var pagingResource = new PagingResource<TResource>
            {
                PageSize = pageSize,
                Page = page,
                Filters = new List<PagingResourceFilter>()
            };

            if (Request.Query.SortKey != null)
            {
                pagingResource.SortKey = Request.Query.SortKey.ToString();

                // For backwards compatibility with v2
                if (Request.Query.SortDir != null)
                {
                    pagingResource.SortDirection = Request.Query.SortDir.ToString()
                                                          .Equals("Asc", StringComparison.InvariantCultureIgnoreCase)
                                                       ? SortDirection.Ascending
                                                       : SortDirection.Descending;
                }

                // v3 uses SortDirection instead of SortDir to be consistent with every other use of it
                if (Request.Query.SortDirection != null)
                {
                    pagingResource.SortDirection = Request.Query.SortDirection.ToString()
                                                          .Equals("ascending", StringComparison.InvariantCultureIgnoreCase)
                                                       ? SortDirection.Ascending
                                                       : SortDirection.Descending;
                }
            }

            // For backwards compatibility with v2
            if (Request.Query.FilterKey != null)
            {
                var filter = new PagingResourceFilter
                             {
                                 Key = Request.Query.FilterKey.ToString()
                             };

                if (Request.Query.FilterValue != null)
                {
                    filter.Value = Request.Query.FilterValue?.ToString();
                }

                pagingResource.Filters.Add(filter);
            }

            // v3 uses filters in key=value format
            foreach (var key in Request.Query)
            {
                if (_excludedKeys.Contains(key))
                {
                    continue;
                }

                pagingResource.Filters.Add(new PagingResourceFilter
                {
                    Key = key,
                    Value = Request.Query[key]
                });
            }

            return pagingResource;
        }
    }
}
