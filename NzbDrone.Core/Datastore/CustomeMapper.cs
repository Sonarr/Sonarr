using System;
using System.Reflection;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public class CustomeMapper : DefaultMapper
    {
        public override Func<object, object> GetToDbConverter(Type sourceType)
        {
            if (sourceType == typeof(QualityTypes))
            {
                return delegate(object s)
                {
                    var source = (QualityTypes)s;
                    return source.Id;
                };
            }

            return base.GetToDbConverter(sourceType);
        }

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

            if ((sourceType == typeof(Int32) || sourceType == typeof(Int64)) && destinationType == typeof(QualityTypes))
            {
                return delegate(object s)
                {
                    int value;
                    Int32.TryParse(s.ToString(), out value);
                    var quality = (QualityTypes)value;
                    return quality;
                };
            }

            return base.GetFromDbConverter(destinationType, sourceType);
        }

        public override Func<object, object> GetFromDbConverter(PropertyInfo propertyInfo, Type sourceType)
        {
            //Only needed if using dynamic as the return type from DB, not implemented currently as it has no use right now
            //if (propertyInfo == null)
            //    return null;

            if (propertyInfo == null) return base.GetFromDbConverter(propertyInfo, sourceType);

            return GetFromDbConverter(propertyInfo.PropertyType, sourceType);
        }
    }


}