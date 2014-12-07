using System;

namespace NzbDrone.Common.Http
{
    public class JsonRpcResponse<T>
    {
        public String Id { get; set; }
        public T Result { get; set; }
        public Object Error { get; set; }
    }
}
