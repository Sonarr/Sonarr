//
// System.Web.HttpUtility/HttpEncoder
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace MonoTorrent
{
    static class UriHelper
    {
        static readonly char [] hexChars = "0123456789abcdef".ToCharArray ();

        public static string UrlEncode (byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException ("bytes");

            var result = new MemoryStream (bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
                UrlEncodeChar ((char)bytes [i], result, false);

            return Encoding.ASCII.GetString (result.ToArray());
        }

        public static byte [] UrlDecode (string s)
        {
            if (null == s)
                return null;

            var e = Encoding.UTF8;
            if (s.IndexOf ('%') == -1 && s.IndexOf ('+') == -1)
                return e.GetBytes (s);

            long len = s.Length;
            var bytes = new List <byte> ();
            int xchar;
            char ch;

            for (int i = 0; i < len; i++) {
                ch = s [i];
                if (ch == '%' && i + 2 < len && s [i + 1] != '%') {
                    if (s [i + 1] == 'u' && i + 5 < len) {
                        // unicode hex sequence
                        xchar = GetChar (s, i + 2, 4);
                        if (xchar != -1) {
                            WriteCharBytes (bytes, (char)xchar, e);
                            i += 5;
                        } else
                            WriteCharBytes (bytes, '%', e);
                    } else if ((xchar = GetChar (s, i + 1, 2)) != -1) {
                        WriteCharBytes (bytes, (char)xchar, e);
                        i += 2;
                    } else {
                        WriteCharBytes (bytes, '%', e);
                    }
                    continue;
                }

                if (ch == '+')
                    WriteCharBytes (bytes, ' ', e);
                else
                    WriteCharBytes (bytes, ch, e);
            }

            return bytes.ToArray ();
        }

        static void UrlEncodeChar (char c, Stream result, bool isUnicode) {
            if (c > ' ' && NotEncoded (c)) {
                result.WriteByte ((byte)c);
                return;
            }
            if (c==' ') {
                result.WriteByte ((byte)'+');
                return;
            }
            if (    (c < '0') ||
                (c < 'A' && c > '9') ||
                (c > 'Z' && c < 'a') ||
                (c > 'z')) {
                if (isUnicode && c > 127) {
                    result.WriteByte ((byte)'%');
                    result.WriteByte ((byte)'u');
                    result.WriteByte ((byte)'0');
                    result.WriteByte ((byte)'0');
                }
                else
                    result.WriteByte ((byte)'%');

                int idx = ((int) c) >> 4;
                result.WriteByte ((byte)hexChars [idx]);
                idx = ((int) c) & 0x0F;
                result.WriteByte ((byte)hexChars [idx]);
            }
            else {
                result.WriteByte ((byte)c);
            }
        }

        static int GetChar (string str, int offset, int length)
        {
            int val = 0;
            int end = length + offset;
            for (int i = offset; i < end; i++) {
                char c = str [i];
                if (c > 127)
                    return -1;

                int current = GetInt ((byte) c);
                if (current == -1)
                    return -1;
                val = (val << 4) + current;
            }

            return val;
        }

        static int GetInt (byte b)
        {
            char c = (char) b;
            if (c >= '0' && c <= '9')
                return c - '0';

            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;

            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;

            return -1;
        }

        static bool NotEncoded (char c)
        {
            return c == '!' || c == '(' || c == ')' || c == '*' || c == '-' || c == '.' || c == '_' || c == '\'';
        }

        static void WriteCharBytes (List<byte> buf, char ch, Encoding e)
        {
            if (ch > 255) {
                foreach (byte b in e.GetBytes (new char[] { ch }))
                    buf.Add (b);
            } else
                buf.Add ((byte)ch);
        }
    }
}
