using System;
using Marr.Data.Converters;
using NzbDrone.Common.Reflection;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Datastore.Converters
{
    public class CommandConverter : EmbeddedDocumentConverter
    {
        public override object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return NullConfig.Instance;
            }

            var stringValue = (string)context.DbValue;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return NullConfig.Instance;
            }

            var ordinal = context.DataRecord.GetOrdinal("Name");
            var contract = context.DataRecord.GetString(ordinal);
            var impType = typeof (Command).Assembly.FindTypeByName(contract + "Command");

            if (impType == null)
            {
                throw new CommandNotFoundException(contract);
            }

            return Json.Deserialize(stringValue, impType);
        }

    }
}