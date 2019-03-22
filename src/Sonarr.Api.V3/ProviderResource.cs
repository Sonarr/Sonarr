using System.Collections.Generic;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.ThingiProvider;
using Sonarr.Http.ClientSchema;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3
{
    public class ProviderResource : RestResource
    {
        public string Name { get; set; }
        public List<Field> Fields { get; set; }
        public string ImplementationName { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public string InfoLink { get; set; }
        public ProviderMessage Message { get; set; }
        public HashSet<int> Tags { get; set; }

        public List<ProviderResource> Presets { get; set; }
    }

    public class ProviderResourceMapper<TProviderResource, TProviderDefinition>
        where TProviderResource : ProviderResource, new()
        where TProviderDefinition : ProviderDefinition, new()
    {
        public virtual TProviderResource ToResource(TProviderDefinition definition)
            
        {
            return new TProviderResource
            {
                Id = definition.Id,

                Name = definition.Name,
                ImplementationName = definition.ImplementationName,
                Implementation = definition.Implementation,
                ConfigContract = definition.ConfigContract,
                Message = definition.Message,
                Tags = definition.Tags,
                Fields = SchemaBuilder.ToSchema(definition.Settings),

                InfoLink = string.Format("https://github.com/Sonarr/Sonarr/wiki/Supported-{0}#{1}",
                    typeof(TProviderResource).Name.Replace("Resource", "s"),
                    definition.Implementation.ToLower())
            };
        }

        public virtual TProviderDefinition ToModel(TProviderResource resource)
        {
            if (resource == null) return default(TProviderDefinition);

            var definition = new TProviderDefinition
            {
                Id = resource.Id,

                Name = resource.Name,
                ImplementationName = resource.ImplementationName,
                Implementation = resource.Implementation,
                ConfigContract = resource.ConfigContract,
                Message = resource.Message,
                Tags = resource.Tags
            };

            var configContract = ReflectionExtensions.CoreAssembly.FindTypeByName(definition.ConfigContract);
            definition.Settings = (IProviderConfig)SchemaBuilder.ReadFromSchema(resource.Fields, configContract);

            return definition;
        }
    }
}