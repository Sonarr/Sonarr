using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.Settings
{
    public abstract class SettingsController<TResource> : RestController<TResource>
        where TResource : RestResource, new()
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;

        protected SettingsController(IConfigFileProvider configFileProvider, IConfigService configService)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
        }

        protected override TResource GetResourceById(int id)
        {
            var resource = ToResource(_configFileProvider, _configService);
            resource.Id = id;

            return resource;
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<TResource> GetConfig()
        {
            return TypedResults.Ok(GetResourceById(1));
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public virtual Results<Accepted<TResource>, NotFound> SaveSettings([FromBody] TResource resource)
        {
            var dictionary = resource.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configFileProvider.SaveConfigDictionary(dictionary);
            _configService.SaveConfigDictionary(dictionary);

            return TypedAccepted(resource.Id);
        }

        protected abstract TResource ToResource(IConfigFileProvider configFile, IConfigService model);
    }
}
