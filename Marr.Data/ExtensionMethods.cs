using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Marr.Data
{
    /// <summary>
    /// This class contains public extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Gets a value from a DbDataReader by using the column name;
        /// </summary>
        public static T GetValue<T>(this DbDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return (T)reader.GetValue(ordinal);
        }
    }
}
