using System;

namespace NzbDrone.Api.ClientSchema
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldDefinitionAttribute : Attribute
    {

        public FieldDefinitionAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; private set; }
        public string Label { get; set; }
        public string HelpText { get; set; }
    }
}