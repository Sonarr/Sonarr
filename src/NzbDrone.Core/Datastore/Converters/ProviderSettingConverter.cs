using System;
using System.Reflection;
using Marr.Data.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NzbDrone.Common.Reflection;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Datastore.Converters
{
    public class ProviderSettingConverter : EmbeddedDocumentConverterBase<ProviderContractResolver>
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
            var contract = context.DataRecord.GetString(ordinal);
            var impType = typeof (IProviderConfig).Assembly.FindTypeByName(contract);

            if (impType == null)
            {
                throw new ConfigContractNotFoundException(contract);
            }

            return Json.Deserialize(stringValue, impType);
        }
    }
     
    public class ProviderContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            var fieldAttribute = member.GetAttribute<FieldDefinitionAttribute>(false);

            if (fieldAttribute != null)
            {
                property.Ignored = !fieldAttribute.Persisted;
            }

            return property;
        }
    }
}