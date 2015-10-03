using System;

namespace NzbDrone.Common.Http
{
    public class JsonRpcResponse<T>
    {
        public string Id { get; set; }
        public T Result { get; set; }
        public object Error { get; set; }
    }
}
