// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Owin.Infrastructure
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "It is instantiated in the static Parse method")]
    internal sealed class ParamDictionary
    {
        private static readonly char[] DefaultParamSeparators = new[] { '&', ';' };
        private static readonly char[] ParamKeyValueSeparator = new[] { '=' };
        private static readonly char[] LeadingWhitespaceChars = new[] { ' ' };

        internal static IEnumerable<KeyValuePair<string, string>> ParseToEnumerable(string value, char[] delimiters = null)
        {
            value = value ?? String.Empty;
            delimiters = delimiters ?? DefaultParamSeparators;

            var items = value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                string[] pair = item.Split(ParamKeyValueSeparator, 2, StringSplitOptions.None);

                string pairKey = UrlDecoder.UrlDecode(pair[0]).TrimStart(LeadingWhitespaceChars);
                string pairValue = pair.Length < 2 ? String.Empty : UrlDecoder.UrlDecode(pair[1]);

                yield return new KeyValuePair<string, string>(pairKey, pairValue);
            }
        }
    }
}
