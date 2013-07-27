using System;

namespace NzbDrone.Core.Annotations
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
        public string HelpLink { get; set; }
        public FieldType Type { get; set; }
        public Type SelectOptions { get; set; }
    }

    public enum FieldType
    {
        Textbox,
        Password,
        Checkbox,
        Select
    }
}