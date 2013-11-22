// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.SignalR.Json
{
    /// <summary>
    /// An implementation of IJsonValue over JSON.NET
    /// </summary>
    internal class JRawValue : IJsonValue
    {
        private readonly string _value;

        public JRawValue(JRaw value)
        {
            _value = value.ToString();
        }

        public object ConvertTo(Type type)
        {
            // A non generic implementation of ToObject<T> on JToken
            using (var jsonReader = new StringReader(_value))
            {
                var settings = new JsonSerializerSettings
                {
                    MaxDepth = 20
                };
                var serializer = JsonSerializer.Create(settings);
                return serializer.Deserialize(jsonReader, type);
            }
        }

        public bool CanConvertTo(Type type)
        {
            // TODO: Implement when we implement better method overload resolution
            return true;
        }
    }
}
