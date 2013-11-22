// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR
{
    public class Cookie
    {
        public Cookie(string name, string value)
            : this(name, value, String.Empty, String.Empty)
        {

        }

        public Cookie(string name, string value, string domain, string path)
        {
            Name = name;
            Value = value;
            Domain = domain;
            Path = path;
        }

        public string Name { get; private set; }
        public string Domain { get; private set; }
        public string Path { get; private set; }
        public string Value { get; private set; }
    }
}
