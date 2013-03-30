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
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Marr.Data.Converters;

namespace Marr.Data.Parameters
{
    /// <summary>
    /// This class allows chaining methods to be called for convenience when adding a parameter.
    /// </summary>
    public class ParameterChainMethods
    {
        /// <summary>
        /// Creates a new parameter and adds it to the command's Parameters collection.
        /// </summary>
        /// <param name="command">The command that the parameter will be added to.</param>
        /// <param name="parameterName">The parameter name.</param>
        public ParameterChainMethods(DbCommand command, string parameterName, object value)
        {
            Parameter = command.CreateParameter();
            Parameter.ParameterName = parameterName;

            // Convert null to DBNull.Value
            if (value == null) 
                value = DBNull.Value;

            Type valueType = value.GetType();

            // Check for a registered IConverter
            IConverter converter = MapRepository.Instance.GetConverter(valueType);
            if (converter != null)
            {
                Parameter.Value = converter.ToDB(value);
            }
            else
            {
                Parameter.Value = value;
            } 

            //// Determine the correct DbType based on the passed in value type
            //IDbTypeBuilder typeBuilder = MapRepository.Instance.DbTypeBuilder;
            //Enum dbType = typeBuilder.GetDbType(valueType);

            //// Set the appropriate DbType property depending on the parameter type
            //typeBuilder.SetDbType(Parameter, dbType);

            command.Parameters.Add(Parameter);
        }

        /// <summary>
        /// Gets a reference to the parameter.
        /// </summary>
        public IDbDataParameter Parameter { get; private set; }

        /// <summary>
        /// Sets the direction of a parameter.
        /// </summary>
        /// <param name="direction">Determines the direction of a parameter.</param>
        /// <returns>Return a ParameterChainMethods object.</returns>
        public ParameterChainMethods Direction(ParameterDirection direction)
        {
            Parameter.Direction = direction;
            return this;
        }

        /// <summary>
        /// Sets the direction of a parameter to 'Output'.
        /// </summary>
        /// <returns></returns>
        public ParameterChainMethods Output()
        {
            Parameter.Direction = ParameterDirection.Output;
            return this;
        }

        public ParameterChainMethods DBType(DbType dbType)
        {
            Parameter.DbType = dbType;
            return this;
        }

        public ParameterChainMethods Size(int size)
        {
            Parameter.Size = size;
            return this;
        }

        public ParameterChainMethods Precision(byte precision)
        {
            Parameter.Precision = precision;
            return this;
        }

        public ParameterChainMethods Scale(byte scale)
        {
            Parameter.Scale = scale;
            return this;
        }
        
        public ParameterChainMethods Name(string name)
        {
            Parameter.ParameterName = name;
            return this;
        }
    }
}
