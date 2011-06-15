using System;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public class CustomeMapper : DefaultMapper
    {
        public override Func<object, object> GetFromDbConverter(DestinationInfo destinationInfo, Type SourceType)
        {

            if ((SourceType == typeof(Int32) || SourceType == typeof(Int64)) && destinationInfo.Type.IsGenericType && destinationInfo.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                if (destinationInfo.Type.GetGenericArguments()[0].IsEnum)
                {
                    return delegate(object s)
                               {
                                   int value;
                                   Int32.TryParse(s.ToString(), out value);
                                   if (value == 0)
                                   {
                                       return null;
                                   }

                                   return (Nullable<DayOfWeek>)value;
                               };
                }
            }

            return base.GetFromDbConverter(destinationInfo, SourceType);
        }
    }
}