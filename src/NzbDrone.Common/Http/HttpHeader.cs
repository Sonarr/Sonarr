using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

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

                if (!ContainsKey("ContentLength"))
                {
                    return null;
                }
                return Convert.ToInt64(this["ContentLength"]);
            }
            set
            {
                this["ContentLength"] = value;
            }
        }
    }
}