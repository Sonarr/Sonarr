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

        public Int32 Order { get; private set; }
        public String Label { get; set; }
        public String HelpText { get; set; }
        public String HelpLink { get; set; }
        public FieldType Type { get; set; }
        public Boolean Advanced { get; set; }
        public Type SelectOptions { get; set; }
    }

    public enum FieldType
    {
        Textbox,
        Password,
        Checkbox,
        Select,
        Path
    }
}