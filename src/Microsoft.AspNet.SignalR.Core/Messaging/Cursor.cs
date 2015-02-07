// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Microsoft.AspNet.SignalR.Messaging
{
    internal unsafe class Cursor
    {
        private static char[] _escapeChars = new[] { '\\', '|', ',' };
        private string _escapedKey;

        public string Key { get; private set; }
        public ulong Id { get; set; }

        public static Cursor Clone(Cursor cursor)
        {
            return new Cursor(cursor.Key, cursor.Id, cursor._escapedKey);
        }

        public Cursor(string key, ulong id)
            : this(key, id, Escape(key))
        {
        }

        public Cursor(string key, ulong id, string minifiedKey)
        {
            Key = key;
            Id = id;
            _escapedKey = minifiedKey;
        }

        public static void WriteCursors(TextWriter textWriter, IList<Cursor> cursors, string prefix)
        {
            textWriter.Write(prefix);

            for (int i = 0; i < cursors.Count; i++)
            {
                if (i > 0)
                {
                    textWriter.Write('|');
                }
                Cursor cursor = cursors[i];
                textWriter.Write(cursor._escapedKey);
                textWriter.Write(',');
                WriteUlongAsHexToBuffer(cursor.Id, textWriter);
            }
        }

        internal static void WriteUlongAsHexToBuffer(ulong value, TextWriter textWriter)
        {
            // This tracks the length of the output and serves as the index for the next character to be written into the pBuffer.
            // The length could reach up to 16 characters, so at least that much space should remain in the pBuffer.
            int length = 0;

            // Write the hex value from left to right into the buffer without zero padding.
            for (int i = 0; i < 16; i++)
            {
                // Convert the first 4 bits of the value to a valid hex character.
                char hexChar = Int32ToHex((int)(value >> 60));
                value <<= 4;

                // Don't increment length if it would just add zero padding
                if (length != 0 || hexChar != '0')
                {
                    textWriter.Write(hexChar);
                    length++;
                }
            }

            if (length == 0)
            {
                textWriter.Write('0');
            }
        }

        private static char Int32ToHex(int value)
        {
            return (value < 10) ? (char)(value + '0') : (char)(value - 10 + 'A');
        }

        private static string Escape(string value)
        {
            // Nothing to do, so bail
            if (value.IndexOfAny(_escapeChars) == -1)
            {
                return value;
            }

            var sb = new StringBuilder();
            // \\ = \
            // \| = |
            // \, = ,
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\\':
                        sb.Append('\\').Append(ch);
                        break;
                    case '|':
                        sb.Append('\\').Append(ch);
                        break;
                    case ',':
                        sb.Append('\\').Append(ch);
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }

            return sb.ToString();
        }

        public static List<Cursor> GetCursors(string cursor, string prefix)
        {
            return GetCursors(cursor, prefix, s => s);
        }

        public static List<Cursor> GetCursors(string cursor, string prefix, Func<string, string> keyMaximizer)
        {
            return GetCursors(cursor, prefix, (key, state) => ((Func<string, string>)state).Invoke(key), keyMaximizer);
        }

        public static List<Cursor> GetCursors(string cursor, string prefix, Func<string, object, string> keyMaximizer, object state)
        {
            // Technically GetCursors should never be called with a null value, so this is extra cautious
            if (String.IsNullOrEmpty(cursor))
            {
                throw new FormatException(Resources.Error_InvalidCursorFormat);
            }

            // If the cursor does not begin with the prefix stream, it isn't necessarily a formatting problem.
            // The cursor with a different prefix might have had different, but also valid, formatting.
            // Null should be returned so new cursors will be generated
            if (!cursor.StartsWith(prefix, StringComparison.Ordinal))
            {
                return null;
            }

            var signals = new HashSet<string>();
            var cursors = new List<Cursor>();
            string currentKey = null;
            string currentEscapedKey = null;
            ulong currentId;
            bool escape = false;
            bool consumingKey = true;
            var sb = new StringBuilder();
            var sbEscaped = new StringBuilder();
            Cursor parsedCursor;

            for (int i = prefix.Length; i < cursor.Length; i++)
            {
                var ch = cursor[i];

                // escape can only be true if we are consuming the key
                if (escape)
                {
                    if (ch != '\\' && ch != ',' && ch != '|')
                    {
                        throw new FormatException(Resources.Error_InvalidCursorFormat);
                    }

                    sb.Append(ch);
                    sbEscaped.Append(ch);
                    escape = false;
                }
                else
                {
                    if (ch == '\\')
                    {
                        if (!consumingKey)
                        {
                            throw new FormatException(Resources.Error_InvalidCursorFormat);
                        }

                        sbEscaped.Append('\\');
                        escape = true;
                    }
                    else if (ch == ',')
                    {
                        if (!consumingKey)
                        {
                            throw new FormatException(Resources.Error_InvalidCursorFormat);
                        }

                        // For now String.Empty is an acceptable key, but this should change once we verify
                        // that empty keys cannot be created legitimately.
                        currentKey = keyMaximizer(sb.ToString(), state);

                        // If the keyMap cannot find a key, we cannot create an array of cursors.
                        // This most likely means there was an AppDomain restart or a misbehaving client.
                        if (currentKey == null)
                        {
                            return null;
                        }
                        // Don't allow duplicate keys
                        if (!signals.Add(currentKey))
                        {
                            throw new FormatException(Resources.Error_InvalidCursorFormat);
                        }

                        currentEscapedKey = sbEscaped.ToString();

                        sb.Clear();
                        sbEscaped.Clear();
                        consumingKey = false;
                    }
                    else if (ch == '|')
                    {
                        if (consumingKey)
                        {
                            throw new FormatException(Resources.Error_InvalidCursorFormat);
                        }

                        ParseCursorId(sb, out currentId);

                        parsedCursor = new Cursor(currentKey, currentId, currentEscapedKey);

                        cursors.Add(parsedCursor);
                        sb.Clear();
                        consumingKey = true;
                    }
                    else
                    {
                        sb.Append(ch);
                        if (consumingKey)
                        {
                            sbEscaped.Append(ch);
                        }
                    }
                }
            }

            if (consumingKey)
            {
                throw new FormatException(Resources.Error_InvalidCursorFormat);
            }

            ParseCursorId(sb, out currentId);

            parsedCursor = new Cursor(currentKey, currentId, currentEscapedKey);

            cursors.Add(parsedCursor);

            return cursors;
        }

        private static void ParseCursorId(StringBuilder sb, out ulong id)
        {
            string value = sb.ToString();
            id = UInt64.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
