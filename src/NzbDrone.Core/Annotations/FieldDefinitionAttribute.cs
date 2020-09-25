using System;
using System.Runtime.CompilerServices;

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
        public string Unit { get; set; }
        public string HelpText { get; set; }
        public string HelpLink { get; set; }
        public FieldType Type { get; set; }
        public bool Advanced { get; set; }
        public Type SelectOptions { get; set; }
        public string SelectOptionsProviderAction { get; set; }
        public string Section { get; set; }
        public HiddenType Hidden { get; set; }
        public PrivacyLevel Privacy { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldOptionAttribute : Attribute
    {
        public FieldOptionAttribute([CallerLineNumber] int order = 0, string label = null)
        {
            Order = order;
            Label = label;
        }

        public int Order { get; private set; }
        public string Label { get; set; }
        public string Hint { get; set; }
    }

    public class FieldSelectOption
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string Hint { get; set; }
        public int? ParentValue { get; set; }
    }

    public enum FieldType
    {
        Textbox,
        Number,
        Password,
        Checkbox,
        Select,
        Path,
        FilePath,
        Tag,
        Action,
        Url,
        Captcha,
        OAuth,
        Device
    }

    public enum HiddenType
    {
        Visible,
        Hidden,
        HiddenIfNotSet
    }

    public enum PrivacyLevel
    {
        Normal,
        Password,
        ApiKey,
        UserName
    }
}
