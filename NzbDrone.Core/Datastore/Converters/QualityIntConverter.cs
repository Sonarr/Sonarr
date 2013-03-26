using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Converters
{
    public class QualityIntConverter : IConverter
    {
        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue == DBNull.Value)
            {
                return Quality.Unknown;
            }

            var val = Convert.ToInt32(dbValue);

            return (Quality)val;
        }

        public object ToDB(object clrValue)
        {
            if(clrValue == null) return 0;

            if(clrValue as Quality == null)
            {
                throw new InvalidOperationException("Attempted to save a quality that isn't really a quality");
            }

            var quality = clrValue as Quality;
            return (int)quality;
        }

        public Type DbType
        {
            get
            {
                return typeof(int);
            }
        }
    }
}