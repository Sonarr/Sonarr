//http://fastjson.codeplex.com/
//http://fastjson.codeplex.com/license

using System;
using System.Collections.Generic;

namespace Exceptron.Client.fastJSON
{
    internal class Getters
    {
        public string Name;
        public JSON.GenericGetter Getter;
        public Type propertyType;
    }

    internal class DatasetSchema
    {
        public List<string> Info { get; set; }
        public string Name { get; set; }
    }
}
