// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Infrastructure;
using Newtonsoft.Json;

namespace Microsoft.AspNet.SignalR.Json
{   
    /// <summary>
    /// A converter for dictionaries that uses a SipHash comparer
    /// </summary>
    internal class SipHashBasedDictionaryConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IDictionary<string, object>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadJsonObject(reader);
        }

        private object ReadJsonObject(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadArray(reader);
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.Value;
                default:
                    throw new NotSupportedException();

            }
        }

        private object ReadArray(JsonReader reader)
        {
            var array = new List<object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    default:
                        object value = ReadJsonObject(reader);

                        array.Add(value);
                        break;
                    case JsonToken.EndArray:
                        return array;
                }
            }

            throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
        }

        private object ReadObject(JsonReader reader)
        {
            var obj = new Dictionary<string, object>(new SipHashBasedStringEqualityComparer());

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
                        }

                        object value = ReadJsonObject(reader);

                        obj[propertyName] = value;
                        break;
                    case JsonToken.EndObject:
                        return obj;
                    default:
                        throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
                        
                }
            }

            throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
