using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public class HttpHeader : Dictionary<string, object>
    {
        public HttpHeader(NameValueCollection headers) : base(StringComparer.OrdinalIgnoreCase)
        {
            foreach (var key in headers.AllKeys)
            {
                this[key] = headers[key];
            }
        }

        public HttpHeader() : base(StringComparer.OrdinalIgnoreCase)
        {

        }

        public long? ContentLength
        {
            get
            {
                if (!ContainsKey("Content-Length"))
                {
                    return null;
                }
                return Convert.ToInt64(this["Content-Length"]);
            }
            set
            {
                this["Content-Length"] = value;
            }
        }

        public string ContentType
        {
            get
            {
                if (!ContainsKey("Content-Type"))
                {
                    return null;
                }
                return this["Content-Type"].ToString();
            }
            set
            {
                this["Content-Type"] = value;
            }
        }

        public string Accept
        {
            get
            {
                if (!ContainsKey("Accept"))
                {
                    return null;
                }
                return this["Accept"].ToString();
            }
            set
            {
                this["Accept"] = value;
            }
        }

        public Encoding GetEncodingFromContentType()
        {
            Encoding encoding = null;

            if (ContentType.IsNotNullOrWhiteSpace())
            {
                var charset = ContentType.ToLowerInvariant()
                    .Split(';', '=', ' ')
                    .SkipWhile(v => v != "charset")
                    .Skip(1).FirstOrDefault();

                if (charset.IsNotNullOrWhiteSpace())
                {
                    encoding = Encoding.GetEncoding(charset);
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
    }
}