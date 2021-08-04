using System.Data;
using System.Text.Json;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Datastore.Converters
{
    public class ProviderSettingConverter : EmbeddedDocumentConverter<IProviderConfig>
    {
        public override IProviderConfig Parse(object value)
        {
            // We can't deserialize based on another column, happens in ProviderRepository instead
            return null;
        }

        public override void SetValue(IDbDataParameter parameter, IProviderConfig value)
        {
            // Cast to object to get all properties written out
            // https://github.com/dotnet/corefx/issues/38650
            parameter.Value = JsonSerializer.Serialize((object)value, SerializerSettings);
        }
    }
}
