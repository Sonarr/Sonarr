using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Reflection;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider
{
    public class ProviderRepository<TProviderDefinition> : BasicRepository<TProviderDefinition>, IProviderRepository<TProviderDefinition>
        where TProviderDefinition : ProviderDefinition, new()
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

        // Asnyc
        protected override async Task<List<TProviderDefinition>> QueryAsync(SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            var type = typeof(TProviderDefinition);
            var sql = builder.Select(type).AddSelectTemplate(type);

            var results = new List<TProviderDefinition>();

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await conn.ExecuteReaderAsync(new CommandDefinition(sql.RawSql, sql.Parameters, cancellationToken: cancellationToken)).ConfigureAwait(false);

            var parser = reader.GetRowParser<TProviderDefinition>(typeof(TProviderDefinition));
            var settingsIndex = reader.GetOrdinal(nameof(ProviderDefinition.Settings));

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var body = reader.IsDBNull(settingsIndex) ? null : reader.GetString(settingsIndex);
                var item = parser(reader);
                var impType = typeof(IProviderConfig).Assembly.FindTypeByName(item.ConfigContract);

                if (body.IsNullOrWhiteSpace() || impType == null)
                {
                    item.Settings = NullConfig.Instance;
                }
                else
                {
                    item.Settings = (IProviderConfig)JsonSerializer.Deserialize(body, impType, _serializerSettings);
                }

                results.Add(item);
            }

            return results;
        }
    }
}
