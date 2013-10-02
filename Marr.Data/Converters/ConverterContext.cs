using System.Data;
using Marr.Data.Mapping;

namespace Marr.Data.Converters
{
    public class ConverterContext
    {
        public ColumnMap ColumnMap { get; set; }
        public object DbValue { get; set; }
        public ColumnMapCollection MapCollection { get; set; }
        public IDataRecord DataRecord { get; set; }
    }
}