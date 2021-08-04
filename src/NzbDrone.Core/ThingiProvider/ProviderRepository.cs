using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Reflection;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider
{
    public class ProviderRepository<TProviderDefinition> : BasicRepository<TProviderDefinition>, IProviderRepository<TProviderDefinition>
        where TProviderDefinition : ProviderDefinition,
            new()
    {
        protected readonly JsonSerializerOptions _serializerSettings;

        protected ProviderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            var serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            serializerSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
            serializerSettings.Converters.Add(new STJTimeSpanConverter());
            serializerSettings.Converters.Add(new STJUtcConverter());

            _serializerSettings = serializerSettings;
        }

        protected override List<TProviderDefinition> Query(SqlBuilder builder)
        {
            var type = typeof(TProviderDefinition);
            var sql = builder.Select(type).AddSelectTemplate(type);

            var results = new List<TProviderDefinition>();

            using (var conn = _database.OpenConnection())
            using (var reader = conn.ExecuteReader(sql.RawSql, sql.Parameters))
            {
                var parser = reader.GetRowParser<TProviderDefinition>(typeof(TProviderDefinition));
                var settingsIndex = reader.GetOrdinal(nameof(ProviderDefinition.Settings));

                while (reader.Read())
                {
                    var body = reader.IsDBNull(settingsIndex) ? null : reader.GetString(settingsIndex);
                    var item = parser(reader);
                    var impType = typeof(IProviderConfig).Assembly.FindTypeByName(item.ConfigContract);

                    if (body.IsNullOrWhiteSpace())
                    {
                        item.Settings = NullConfig.Instance;
                    }
                    else
                    {
                        item.Settings = (IProviderConfig)JsonSerializer.Deserialize(body, impType, _serializerSettings);
                    }

                    results.Add(item);
                }
            }

            return results;
        }
    }
}
