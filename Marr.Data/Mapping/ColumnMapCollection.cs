/*  Copyright (C) 2008 - 2011 Jordan Marr

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library. If not, see <http://www.gnu.org/licenses/>. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Marr.Data.Mapping
{
    /// <summary>
    /// This class contains a list of column mappings.
    /// It also provides various methods to filter the collection.
    /// </summary>
    public class ColumnMapCollection : List<ColumnMap>
    {
        #region - Filters -

        public ColumnMap GetByColumnName(string columnName)
        {
            return this.Find(m => m.ColumnInfo.Name == columnName);
        }

        public ColumnMap GetByFieldName(string fieldName)
        {
            return this.Find(m => m.FieldName == fieldName);
        }

        /// <summary>
        /// Iterates through all fields marked as return values.
        /// </summary>
        public IEnumerable<ColumnMap> ReturnValues
        {
            get
            {
                foreach (ColumnMap map in this)
                    if (map.ColumnInfo.ReturnValue)
                        yield return map;
            }
        }

        /// <summary>
        /// Iterates through all fields that are not return values.
        /// </summary>
        public ColumnMapCollection NonReturnValues
        {
            get
            {
                ColumnMapCollection collection = new ColumnMapCollection();

                foreach (ColumnMap map in this)
                    if (!map.ColumnInfo.ReturnValue)
                        collection.Add(map);

                return collection;
            }
        }

        /// <summary>
        /// Iterates through all fields marked as Output parameters or InputOutput.
        /// </summary>
        public IEnumerable<ColumnMap> OutputFields
        {
            get
            {
                foreach (ColumnMap map in this)
                    if (map.ColumnInfo.ParamDirection == ParameterDirection.InputOutput ||
                        map.ColumnInfo.ParamDirection == ParameterDirection.Output)
                        yield return map;
            }
        }

        /// <summary>
        /// Iterates through all fields marked as primary keys.
        /// </summary>
        public ColumnMapCollection PrimaryKeys
        {
            get
            {
                ColumnMapCollection keys = new ColumnMapCollection();
                foreach (ColumnMap map in this)
                    if (map.ColumnInfo.IsPrimaryKey)
                        keys.Add(map);

                return keys;
            }
        }

        /// <summary>
        /// Parses and orders the parameters from the query text.  
        /// Filters the list of mapped columns to match the parameters found in the sql query.
        /// All parameters starting with the '@' or ':' symbol are matched and returned.
        /// </summary>
        /// <param name="command">The command and parameters that are being parsed.</param>
        /// <returns>A list of mapped columns that are present in the sql statement as parameters.</returns>
        public ColumnMapCollection OrderParameters(DbCommand command)
        {
            if (command.CommandType == CommandType.Text && this.Count > 0)
            {
                string commandTypeString = command.GetType().ToString();
                if (commandTypeString.Contains("Oracle") || commandTypeString.Contains("OleDb"))
                {
                    ColumnMapCollection columns = new ColumnMapCollection();

                    // Find all @Parameters contained in the sql statement
                    string paramPrefix = commandTypeString.Contains("Oracle") ? ":" : "@";
                    string regexString = string.Format(@"{0}[\w-]+", paramPrefix);
                    Regex regex = new Regex(regexString);
                    foreach (Match m in regex.Matches(command.CommandText))
                    {
                        ColumnMap matchingColumn = this.Find(c => string.Concat(paramPrefix, c.ColumnInfo.Name.ToLower()) == m.Value.ToLower());
                        if (matchingColumn != null)
                            columns.Add(matchingColumn);
                    }

                    return columns;
                }
            }

            return this;
        }


        #endregion

        #region - Actions -

        /// <summary>
        /// Set's each column's altname as the given prefix + the column name.
        /// Ex: 
        /// Original column name: "ID"
        /// Passed in prefix: "PRODUCT_"
        /// Generated AltName: "PRODUCT_ID"
        /// </summary>
        /// <param name="prefix">The given prefix.</param>
        /// <returns></returns>
        public ColumnMapCollection PrefixAltNames(string prefix)
        {
            this.ForEach(c => c.ColumnInfo.AltName = c.ColumnInfo.Name.Insert(0, prefix));
            return this;
        }

        /// <summary>
        /// Set's each column's altname as the column name + the given prefix.
        /// Ex: 
        /// Original column name: "ID"
        /// Passed in suffix: "_PRODUCT"
        /// Generated AltName: "ID_PRODUCT"
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public ColumnMapCollection SuffixAltNames(string suffix)
        {
            this.ForEach(c => c.ColumnInfo.AltName = c.ColumnInfo.Name + suffix);
            return this;
        }

        #endregion
    }
}
