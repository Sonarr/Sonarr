using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public static class WebHeaderCollectionExtensions
    {
        public static NameValueCollection ToNameValueCollection(this HttpHeaders headers)
        {
            var result = new NameValueCollection();
            foreach (var header in headers)
            {
                result.Add(header.Key, header.Value.ConcatToString(";"));
            }

            return result;
        }
    }

    public class HttpHeader : NameValueCollection, IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        public HttpHeader(NameValueCollection headers)
            : base(headers)
        {
        }

        public HttpHeader(HttpHeaders headers)
            : base(headers.ToNameValueCollection())
        {
        }

        public HttpHeader()
        {
        }

        public bool ContainsKey(string key)
        {
            key = key.ToLowerInvariant();
            return AllKeys.Any(v => v.ToLowerInvariant() == key);
        }

        public string GetSingleValue(string key)
        {
            var values = GetValues(key);
            if (values == null || values.Length == 0)
            {
                return null;
            }

            if (values.Length > 1)
            {
                throw new ApplicationException($"Expected {key} to occur only once, but was {values.Join("|")}.");
            }

            return values[0];
        }

        protected T? GetSingleValue<T>(string key, Func<string, T> converter)
            where T : struct
        {
            var value = GetSingleValue(key);
            if (value == null)
            {
                return null;
            }

            return converter(value);
        }

        protected void SetSingleValue(string key, string value)
        {
            if (value == null)
            {
                Remove(key);
            }
            else
            {
                Set(key, value);
            }
        }

        protected void SetSingleValue<T>(string key, T? value, Func<T, string> converter = null)
            where T : struct
        {
            if (!value.HasValue)
            {
                Remove(key);
            }
            else if (converter != null)
            {
                Set(key, converter(value.Value));
            }
            else
            {
                Set(key, value.Value.ToString());
            }
        }

        public long? ContentLength
        {
            get
            {
                return GetSingleValue("Content-Length", Convert.ToInt64);
            }
            set
            {
                SetSingleValue("Content-Length", value);
            }
        }

        public string ContentType
        {
            get
            {
                return GetSingleValue("Content-Type");
            }
            set
            {
                SetSingleValue("Content-Type", value);
            }
        }

        public string Accept
        {
            get
            {
                return GetSingleValue("Accept");
            }
            set
            {
                SetSingleValue("Accept", value);
            }
        }

        public new IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return AllKeys.SelectMany(GetValues, (k, c) => new KeyValuePair<string, string>(k, c)).ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        public Encoding GetEncodingFromContentType()
        {
            return GetEncodingFromContentType(ContentType ?? string.Empty);
        }

        public static Encoding GetEncodingFromContentType(string contentType)
        {
            Encoding encoding = null;

            if (contentType.IsNotNullOrWhiteSpace())
            {
                var charset = contentType.ToLowerInvariant()
                    .Split(';', '=', ' ')
                    .SkipWhile(v => v != "charset")
                    .Skip(1).FirstOrDefault();

                if (charset.IsNotNullOrWhiteSpace())
                {
                    encoding = Encoding.GetEncoding(charset.Replace("\"", ""));
                }
            }

            if (encoding == null)
            {
                // TODO: Find encoding by Byte order mask.
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return encoding;
        }

        public static DateTime ParseDateTime(string value)
        {
            return DateTime.ParseExact(value, "R", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
        }

        public static List<KeyValuePair<string, string>> ParseCookies(string cookies)
        {
            return cookies.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(v => v.Trim().Split('='))
                          .Select(v => new KeyValuePair<string, string>(v[0], v[1]))
                          .ToList();
        }
    }
}
