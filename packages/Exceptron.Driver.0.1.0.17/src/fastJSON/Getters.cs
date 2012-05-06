//http://fastjson.codeplex.com/

using System;
using System.Collections.Generic;
using Exceptron.Driver.fastJSON;

namespace Exceptron.Driver.fastJSON
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
