// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    // A string equality comparer based on the SipHash-2-4 algorithm. Key differences:
    // (a) we output 32-bit hashes instead of 64-bit hashes, and
    // (b) we don't care about endianness since hashes are used only in hash tables
    //     and aren't returned to user code.
    //
    // Meant to serve as a replacement for StringComparer.Ordinal.
    // Derivative work of https://github.com/tanglebones/ch-siphash.
    internal unsafe sealed class SipHashBasedStringEqualityComparer : IEqualityComparer<string>
    {
        private static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        // the 128-bit secret key
        private readonly ulong _k0;
        private readonly ulong _k1;

        public SipHashBasedStringEqualityComparer()
            : this(GenerateRandomKeySegment(), GenerateRandomKeySegment())
        {
        }

        // for unit testing
        internal SipHashBasedStringEqualityComparer(ulong k0, ulong k1)
        {
            _k0 = k0;
            _k1 = k1;
        }

        public bool Equals(string x, string y)
        {
            return String.Equals(x, y);
        }

        private static ulong GenerateRandomKeySegment()
        {
            byte[] bytes = new byte[sizeof(ulong)];
            _rng.GetBytes(bytes);
            return (ulong)BitConverter.ToInt64(bytes, 0);
        }

        public int GetHashCode(string obj)
        {
            if (obj == null)
            {
                return 0;
            }

            fixed (char* pChars = obj)
            {
                // treat input as an opaque blob, convert char count to byte count
                return GetHashCode((byte*)pChars, checked((uint)obj.Length * sizeof(char)));
            }
        }

        // for unit testing
        internal int GetHashCode(byte* bytes, uint len)
        {
            // Assume SipHash-2-4 is a strong PRF, therefore truncation to 32 bits is acceptable.
            return (int)SipHash_2_4_UlongCast_ForcedInline(bytes, len, _k0, _k1);
        }

        private static unsafe ulong SipHash_2_4_UlongCast_ForcedInline(byte* finb, uint inlen, ulong k0, ulong k1)
        {
            var v0 = 0x736f6d6570736575 ^ k0;
            var v1 = 0x646f72616e646f6d ^ k1;
            var v2 = 0x6c7967656e657261 ^ k0;
            var v3 = 0x7465646279746573 ^ k1;

            var b = ((ulong)inlen) << 56;

            if (inlen > 0)
            {
                var inb = finb;
                var left = inlen & 7;
                var end = inb + inlen - left;
                var linb = (ulong*)finb;
                var lend = (ulong*)end;
                for (; linb < lend; ++linb)
                {
                    v3 ^= *linb;

                    v0 += v1;
                    v1 = (v1 << 13) | (v1 >> (64 - 13));
                    v1 ^= v0;
                    v0 = (v0 << 32) | (v0 >> (64 - 32));

                    v2 += v3;
                    v3 = (v3 << 16) | (v3 >> (64 - 16));
                    v3 ^= v2;

                    v0 += v3;
                    v3 = (v3 << 21) | (v3 >> (64 - 21));
                    v3 ^= v0;

                    v2 += v1;
                    v1 = (v1 << 17) | (v1 >> (64 - 17));
                    v1 ^= v2;
                    v2 = (v2 << 32) | (v2 >> (64 - 32));
                    v0 += v1;
                    v1 = (v1 << 13) | (v1 >> (64 - 13));
                    v1 ^= v0;
                    v0 = (v0 << 32) | (v0 >> (64 - 32));

                    v2 += v3;
                    v3 = (v3 << 16) | (v3 >> (64 - 16));
                    v3 ^= v2;

                    v0 += v3;
                    v3 = (v3 << 21) | (v3 >> (64 - 21));
                    v3 ^= v0;

                    v2 += v1;
                    v1 = (v1 << 17) | (v1 >> (64 - 17));
                    v1 ^= v2;
                    v2 = (v2 << 32) | (v2 >> (64 - 32));

                    v0 ^= *linb;
                }
                for (var i = 0; i < left; ++i)
                {
                    b |= ((ulong)end[i]) << (8 * i);
                }
            }

            v3 ^= b;
            v0 += v1;
            v1 = (v1 << 13) | (v1 >> (64 - 13));
            v1 ^= v0;
            v0 = (v0 << 32) | (v0 >> (64 - 32));

            v2 += v3;
            v3 = (v3 << 16) | (v3 >> (64 - 16));
            v3 ^= v2;

            v0 += v3;
            v3 = (v3 << 21) | (v3 >> (64 - 21));
            v3 ^= v0;

            v2 += v1;
            v1 = (v1 << 17) | (v1 >> (64 - 17));
            v1 ^= v2;
            v2 = (v2 << 32) | (v2 >> (64 - 32));
            v0 += v1;
            v1 = (v1 << 13) | (v1 >> (64 - 13));
            v1 ^= v0;
            v0 = (v0 << 32) | (v0 >> (64 - 32));

            v2 += v3;
            v3 = (v3 << 16) | (v3 >> (64 - 16));
            v3 ^= v2;

            v0 += v3;
            v3 = (v3 << 21) | (v3 >> (64 - 21));
            v3 ^= v0;

            v2 += v1;
            v1 = (v1 << 17) | (v1 >> (64 - 17));
            v1 ^= v2;
            v2 = (v2 << 32) | (v2 >> (64 - 32));
            v0 ^= b;
            v2 ^= 0xff;

            v0 += v1;
            v1 = (v1 << 13) | (v1 >> (64 - 13));
            v1 ^= v0;
            v0 = (v0 << 32) | (v0 >> (64 - 32));

            v2 += v3;
            v3 = (v3 << 16) | (v3 >> (64 - 16));
            v3 ^= v2;

            v0 += v3;
            v3 = (v3 << 21) | (v3 >> (64 - 21));
            v3 ^= v0;

            v2 += v1;
            v1 = (v1 << 17) | (v1 >> (64 - 17));
            v1 ^= v2;
            v2 = (v2 << 32) | (v2 >> (64 - 32));
            v0 += v1;
            v1 = (v1 << 13) | (v1 >> (64 - 13));
            v1 ^= v0;
            v0 = (v0 << 32) | (v0 >> (64 - 32));

            v2 += v3;
            v3 = (v3 << 16) | (v3 >> (64 - 16));
            v3 ^= v2;

            v0 += v3;
            v3 = (v3 << 21) | (v3 >> (64 - 21));
            v3 ^= v0;

            v2 += v1;
            v1 = (v1 << 17) | (v1 >> (64 - 17));
            v1 ^= v2;
            v2 = (v2 << 32) | (v2 >> (64 - 32));
            v0 += v1;
            v1 = (v1 << 13) | (v1 >> (64 - 13));
            v1 ^= v0;
            v0 = (v0 << 32) | (v0 >> (64 - 32));

            v2 += v3;
            v3 = (v3 << 16) | (v3 >> (64 - 16));
            v3 ^= v2;

            v0 += v3;
            v3 = (v3 << 21) | (v3 >> (64 - 21));
            v3 ^= v0;

            v2 += v1;
            v1 = (v1 << 17) | (v1 >> (64 - 17));
            v1 ^= v2;
            v2 = (v2 << 32) | (v2 >> (64 - 32));
            v0 += v1;
            v1 = (v1 << 13) | (v1 >> (64 - 13));
            v1 ^= v0;
            v0 = (v0 << 32) | (v0 >> (64 - 32));

            v2 += v3;
            v3 = (v3 << 16) | (v3 >> (64 - 16));
            v3 ^= v2;

            v0 += v3;
            v3 = (v3 << 21) | (v3 >> (64 - 21));
            v3 ^= v0;

            v2 += v1;
            v1 = (v1 << 17) | (v1 >> (64 - 17));
            v1 ^= v2;
            v2 = (v2 << 32) | (v2 >> (64 - 32));

            return v0 ^ v1 ^ v2 ^ v3;
        }
    }
}
