using System;
using Marr.Data.Converters;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Reflection;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Datastore.Converters
{
    public class CommandConverter : EmbeddedDocumentConverter
    {
        public override object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return null;
            }

            var stringValue = (string)context.DbValue;

            if (stringValue.IsNullOrWhiteSpace())
            {
                return null;
            }

            var ordinal = context.DataRecord.GetOrdinal("Name");
            var contract = context.DataRecord.GetString(ordinal);
            var impType = typeof(Command).Assembly.FindTypeByName(contract + "Command");

            if (impType == null)
            {
                var result = Json.Deserialize<UnknownCommand>(stringValue);

                result.ContractName = contract;

                return result;
            }

            return Json.Deserialize(stringValue, impType);
        }
    }
}
