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
        protected NzbDroneRestModule()
            : this(new TResource().ResourceName)
        {

        }

        protected NzbDroneRestModule(string resource)
            : base("/api/" + resource.Trim('/'))
        {
            PostValidator.RuleFor(r => r.Id).IsZero();
            PutValidator.RuleFor(r => r.Id).ValidId();
        }


        protected TResource Apply<TModel>(Func<TModel, TModel> function, TResource resource) where TModel : ModelBase, new()
        {
            var model = resource.InjectTo<TModel>();
            function(model);
            return model.InjectTo<TResource>();
        }

        protected List<TResource> ApplyToList<TModel>(Func<IEnumerable<TModel>> function) where TModel : ModelBase, new()
        {
            var modelList = function();
            return modelList.InjectTo<List<TResource>>();
        }

        protected TResource Apply<TModel>(Func<TModel> function) where TModel : ModelBase, new()
        {
            var modelList = function();
            return modelList.InjectTo<TResource>();
        }

        protected TResource Apply<TModel>(Func<int, TModel> action, int id) where TModel : ModelBase, new()
        {
            var model = action(id);
            return model.InjectTo<TResource>();
        }

    }
}