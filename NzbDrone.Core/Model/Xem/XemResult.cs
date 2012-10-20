using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model.Xem
{
    public class XemResult<T>
    {
        public string Result { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}
