using System;

namespace Sonarr.Http.ClientSchema
{
    public class FieldMapping
    {
        public Field Field { get; set; }
        public Type PropertyType { get; set; }
        public Func<object, object> GetterFunc { get; set; }
        public Action<object, object> SetterFunc { get; set; }
    }
}
