using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NzbDrone.Common.Http
{
    public class HttpHeader : Dictionary<string, object>
    {
        public HttpHeader(NameValueCollection headers)
        {
            foreach (var key in headers.AllKeys)
            {
                this[key] = headers[key];
            }
        }

        public HttpHeader()
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
    }
}