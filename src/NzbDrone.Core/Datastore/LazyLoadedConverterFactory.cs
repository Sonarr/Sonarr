using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.Datastore
{
    public class LazyLoadedConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            return typeToConvert.GetGenericTypeDefinition() == typeof(LazyLoaded<>);
        }

        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            var childType = type.GetGenericArguments()[0];

            return (JsonConverter)Activator.CreateInstance(
                typeof(LazyLoadedConverter<>).MakeGenericType(childType),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);
        }

        private class LazyLoadedConverter<TChild> : JsonConverter<LazyLoaded<TChild>>
        {
            private readonly JsonConverter<TChild> _childConverter;
            private readonly Type _childType;

            public LazyLoadedConverter(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                _childConverter = (JsonConverter<TChild>)options
                    .GetConverter(typeof(TChild));

                // Cache the type.
                _childType = typeof(TChild);
            }

            public override LazyLoaded<TChild> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                TChild value;
                if (_childConverter != null)
                {
                    reader.Read();
                    value = _childConverter.Read(ref reader, _childType, options);
                }
                else
                {
                    value = JsonSerializer.Deserialize<TChild>(ref reader, options);
                }

                if (value != null)
                {
                    return new LazyLoaded<TChild>(value);
                }
                else
                {
                    return null;
                }
            }

            public override void Write(Utf8JsonWriter writer, LazyLoaded<TChild> value, JsonSerializerOptions options)
            {
                if (value.IsLoaded)
                {
                    if (_childConverter != null)
                    {
                        _childConverter.Write(writer, value.Value, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, value.Value, options);
                    }
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
    }
}
