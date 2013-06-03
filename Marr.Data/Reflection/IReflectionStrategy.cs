using System;

namespace Marr.Data.Reflection
{
    public interface IReflectionStrategy
    {
        object GetFieldValue(object entity, string fieldName);

        GetterDelegate BuildGetter(Type type, string memberName);
        SetterDelegate BuildSetter(Type type, string memberName);

        object CreateInstance(Type type);
    }

    public delegate void SetterDelegate(object instance, object value);
    public delegate object GetterDelegate(object instance);
}
