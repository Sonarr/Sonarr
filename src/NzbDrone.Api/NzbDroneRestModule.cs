using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Api.Validation;
using NzbDrone.Core.Datastore;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api
{
    public abstract class NzbDroneRestModule<TResource> : RestModule<TResource> where TResource : RestResource, new()
    {
        protected string Resource { get; private set; }

        protected NzbDroneRestModule()
            : this(new TResource().ResourceName)
        {
        }

        protected NzbDroneRestModule(string resource)
            : base("/api/" + resource.Trim('/'))
        {
            Resource = resource;
            PostValidator.RuleFor(r => r.Id).IsZero();
            PutValidator.RuleFor(r => r.Id).ValidId();
        }

        protected int GetNewId<TModel>(Func<TModel, TModel> function, TResource resource) where TModel : ModelBase, new()
        {
            var model = resource.InjectTo<TModel>();
            function(model);
            return model.Id;
        }

        protected List<TResource> ToListResource<TModel>(Func<IEnumerable<TModel>> function) where TModel : class
        {
            var modelList = function();
            return modelList.InjectTo<List<TResource>>();
        }

        protected PagingResource<TResource> ApplyToPage<TModel>(Func<PagingSpec<TModel>, PagingSpec<TModel>> function, PagingSpec<TModel> pagingSpec) where TModel : ModelBase, new()
        {
            pagingSpec = function(pagingSpec);

            return new PagingResource<TResource>
                       {
                           Page = pagingSpec.Page,
                           PageSize = pagingSpec.PageSize,
                           SortDirection = pagingSpec.SortDirection,
                           SortKey = pagingSpec.SortKey,
                           TotalRecords = pagingSpec.TotalRecords,
                           Records = pagingSpec.Records.InjectTo<List<TResource>>()
                       };
        }
    }
}