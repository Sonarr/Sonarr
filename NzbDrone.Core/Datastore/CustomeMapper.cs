using System;
using System.Reflection;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public class CustomeMapper : DefaultMapper
    {
        public override Func<object, object> GetFromDbConverter(Type destinationType, Type sourceType)
        {

            if ((sourceType == typeof(Int32) || sourceType == typeof(Int64)) && destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                Type genericArgument = destinationType.GetGenericArguments()[0];
                if (genericArgument == typeof(DayOfWeek))
                {
                    return delegate(object s)
                               {
                                   int value;
                                   Int32.TryParse(s.ToString(), out value);
                                   return (DayOfWeek?)value;
                               };
                }

                return delegate(object s)
                           {
                               int value;
                               Int32.TryParse(s.ToString(), out value);
                               return value;
                           };
            }

            return base.GetFromDbConverter(destinationType, sourceType);
        }

        public override Func<object, object> GetFromDbConverter(PropertyInfo propertyInfo, Type sourceType)
        {
            return GetFromDbConverter(propertyInfo.PropertyType, sourceType);
        }
    }

    
}