using System.Data;
using System.Text.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Datastore.Converters
{
    public class CommandConverter : EmbeddedDocumentConverter<Command>
    {
        public override Command Parse(object value)
        {
            var stringValue = (string)value;

            if (stringValue.IsNullOrWhiteSpace())
            {
                return null;
            }

            string contract;
            using (var body = JsonDocument.Parse(stringValue))
            {
                contract = body.RootElement.GetProperty("name").GetString();
            }

            var impType = typeof(Command).Assembly.FindTypeByName(contract + "Command");

            if (impType == null)
            {
                var result = JsonSerializer.Deserialize<UnknownCommand>(stringValue, SerializerSettings);

                result.ContractName = contract;

                return result;
            }

            return (Command)JsonSerializer.Deserialize(stringValue, impType, SerializerSettings);
        }

        public override void SetValue(IDbDataParameter parameter, Command value)
        {
            // Cast to object to get all properties written out
            // https://github.com/dotnet/corefx/issues/38650
            parameter.Value = value == null ? null : JsonSerializer.Serialize((object)value, SerializerSettings);
        }
    }
}
