using System;
using Marr.Data.Converters;
using NzbDrone.Common.Reflection;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Datastore.Converters
{
    public class ProviderSettingConverter : EmbeddedDocumentConverter
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

            var ordinal = context.DataRecord.GetOrdinal("ConfigContract");

            var implementation = context.DataRecord.GetString(ordinal);


            var impType = typeof (IProviderConfig).Assembly.FindTypeByName(implementation);

            return Json.Deserialize(stringValue, impType);
        }

    }
}