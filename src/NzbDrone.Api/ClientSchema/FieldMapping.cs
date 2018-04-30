using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.ClientSchema
{
    public class FieldMapping
    {
        public Field Field { get; set; }
        public Type PropertyType { get; set; }
        public Func<object, object> GetterFunc { get; set; }
        public Action<object, object> SetterFunc { get; set; }
    }
}
