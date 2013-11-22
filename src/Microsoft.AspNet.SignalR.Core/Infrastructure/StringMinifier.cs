// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    internal class StringMinifier : IStringMinifier
    {
        private readonly ConcurrentDictionary<string, string> _stringMinifier = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> _stringMaximizer = new ConcurrentDictionary<string, string>();
        private int _lastMinifiedKey = -1;

        private readonly Func<string, string> _createMinifiedString;

        public StringMinifier()
        {
            _createMinifiedString = CreateMinifiedString;
        }

        public string Minify(string fullString)
        {
            return _stringMinifier.GetOrAdd(fullString, _createMinifiedString);
        }

        public string Unminify(string minifiedString)
        {
            string result;
            _stringMaximizer.TryGetValue(minifiedString, out result);
            return result;
        }

        public void RemoveUnminified(string fullString)
        {
            string minifiedString;
            if (_stringMinifier.TryRemove(fullString, out minifiedString))
            {
                string value;
                _stringMaximizer.TryRemove(minifiedString, out value);
            }
        }

        private string CreateMinifiedString(string fullString)
        {
            var minString = GetStringFromInt((uint)Interlocked.Increment(ref _lastMinifiedKey));
            _stringMaximizer.TryAdd(minString, fullString);
            return minString;
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "This is a valid exception to throw.")]
        private static char GetCharFromSixBitInt(uint num)
        {
            if (num < 26)
            {
                return (char)(num + 'A');
            }
            if (num < 52)
            {
                return (char)(num - 26 + 'a');
            }
            if (num < 62)
            {
                return (char)(num - 52 + '0');
            }
            if (num == 62)
            {
                return '_';
            }
            if (num == 63)
            {
                return ':';
            }
            throw new IndexOutOfRangeException();
        }

        private static string GetStringFromInt(uint num)
        {
            const int maxSize = 6;

            // Buffer must be large enough to store any 32 bit uint at 6 bits per character
            var buffer = new char[maxSize];
            var index = maxSize;
            do
            {
                // Append next 6 bits of num
                buffer[--index] = GetCharFromSixBitInt(num & 0x3f);
                num >>= 6;

                // Don't pad output string, but ensure at least one character is written
            } while (num != 0);

            return new string(buffer, index, maxSize - index);
        }
    }
}
