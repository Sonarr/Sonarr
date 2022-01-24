using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NzbDrone.Core.CustomFormats;
using Sonarr.Http.ClientSchema;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.CustomFormats
{
    public class CustomFormatResource : RestResource
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public override int Id { get; set; }
        public string Name { get; set; }
        public bool IncludeCustomFormatWhenRenaming { get; set; }
        public List<CustomFormatSpecificationSchema> Specifications { get; set; }
    }

    public static class CustomFormatResourceMapper
    {
        public static CustomFormatResource ToResource(this CustomFormat model)
        {
            return new CustomFormatResource
            {
                Id = model.Id,
                Name = model.Name,
                IncludeCustomFormatWhenRenaming = model.IncludeCustomFormatWhenRenaming,
                Specifications = model.Specifications.Select(x => x.ToSchema()).ToList()
            };
        }

        public static List<CustomFormatResource> ToResource(this IEnumerable<CustomFormat> models)
        {
            return models.Select(m => m.ToResource()).ToList();
        }

        public static CustomFormat ToModel(this CustomFormatResource resource, List<ICustomFormatSpecification> specifications)
        {
            return new CustomFormat
            {
                Id = resource.Id,
                Name = resource.Name,
                IncludeCustomFormatWhenRenaming = resource.IncludeCustomFormatWhenRenaming,
                Specifications = resource.Specifications.Select(x => MapSpecification(x, specifications)).ToList()
            };
        }

        private static ICustomFormatSpecification MapSpecification(CustomFormatSpecificationSchema resource, List<ICustomFormatSpecification> specifications)
        {
            var type = specifications.SingleOrDefault(x => x.GetType().Name == resource.Implementation).GetType();
            var spec = (ICustomFormatSpecification)SchemaBuilder.ReadFromSchema(resource.Fields, type);
            spec.Name = resource.Name;
            spec.Negate = resource.Negate;
            spec.Required = resource.Required;
            return spec;
        }
    }
}
