using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class JsonRpcResponse<T>
    {
        public String Id { get; set; }
        public T Result { get; set; }
        public Object Error { get; set; }
    }
}
