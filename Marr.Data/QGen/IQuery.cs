using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data.QGen
{
    internal interface IQuery
    {
        /// <summary>
        /// Generates a SQL query for a given entity.
        /// </summary>
        /// <returns></returns>
        string Generate();
    }
}
