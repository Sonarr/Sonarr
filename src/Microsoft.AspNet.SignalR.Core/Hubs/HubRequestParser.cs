// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNet.SignalR.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.SignalR.Hubs
{
    internal class HubRequestParser : IHubRequestParser
    {
        private static readonly IJsonValue[] _emptyArgs = new IJsonValue[0];

        public HubRequest Parse(string data)
        {
            var serializer = new JsonNetSerializer();
            var deserializedData = serializer.Parse<HubInvocation>(data);

            var request = new HubRequest();

            request.Hub = deserializedData.Hub;
            request.Method = deserializedData.Method;
            request.Id = deserializedData.Id;
            request.State = GetState(deserializedData);
            request.ParameterValues = (deserializedData.Args != null) ? deserializedData.Args.Select(value => new JRawValue(value)).ToArray() : _emptyArgs;

            return request;
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This type is used for deserialzation")]
        private class HubInvocation
        {
            [JsonProperty("H")]
            public string Hub { get; set; }
            [JsonProperty("M")]
            public string Method { get; set; }
            [JsonProperty("I")]
            public string Id { get; set; }
            [JsonProperty("S")]
            public JRaw State { get; set; }
            [JsonProperty("A")]
            public JRaw[] Args { get; set; }
        }

        private static IDictionary<string, object> GetState(HubInvocation deserializedData)
        {
            if (deserializedData.State == null)
            {
                return new Dictionary<string, object>();
            }

            // Get the raw JSON string and check if it's over 4K
            string json = deserializedData.State.ToString();

            if (json.Length > 4096)
            {
                throw new InvalidOperationException(Resources.Error_StateExceededMaximumLength);
            }

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new SipHashBasedDictionaryConverter());
            var serializer = new JsonNetSerializer(settings);
            return serializer.Parse<IDictionary<string, object>>(json);
        }
    }
}
