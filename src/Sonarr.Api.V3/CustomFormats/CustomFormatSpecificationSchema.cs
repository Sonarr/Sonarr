using System.Collections.Generic;
using NzbDrone.Core.CustomFormats;
using Sonarr.Http.ClientSchema;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.CustomFormats
{
    public class CustomFormatSpecificationSchema : RestResource
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string ImplementationName { get; set; }
        public string InfoLink { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }
        public List<Field> Fields { get; set; }
        public List<CustomFormatSpecificationSchema> Presets { get; set; }
    }

    public static class CustomFormatSpecificationSchemaMapper
    {
        public static CustomFormatSpecificationSchema ToSchema(this ICustomFormatSpecification model)
        {
            return new CustomFormatSpecificationSchema
            {
                Name = model.Name,
                Implementation = model.GetType().Name,
                ImplementationName = model.ImplementationName,
                InfoLink = model.InfoLink,
                Negate = model.Negate,
                Required = model.Required,
                Fields = SchemaBuilder.ToSchema(model)
            };
        }
    }
}
