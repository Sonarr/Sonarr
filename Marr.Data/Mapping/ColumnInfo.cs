using System.Data;

namespace Marr.Data.Mapping
{
    public class ColumnInfo : IColumnInfo
    {
        public ColumnInfo()
        {
            IsPrimaryKey = false;
            IsAutoIncrement = false;
            ReturnValue = false;
            ParamDirection = ParameterDirection.Input;
        }

        public string Name { get; set; }
        public string AltName { get; set; }
        public int Size { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool ReturnValue { get; set; }
        public ParameterDirection ParamDirection { get; set; }

        public string TryGetAltName()
        {
            if (!string.IsNullOrEmpty(AltName) && AltName != Name)
            {
                return AltName;
            }
            else
            {
                return Name;
            }
        }
    }
}
