using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data.Reflection
{
    public interface IReflectionStrategy
    {
        void SetFieldValue<T>(T entity, string fieldName, object val);
        object GetFieldValue(object entity, string fieldName);
        object CreateInstance(Type type);
    }
}
