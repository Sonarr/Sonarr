using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Helpers.Validation
{
    public class RequiredIfAnyAttribute : ValidationAttribute, IClientValidatable
    {
        private RequiredAttribute _innerAttribute = new RequiredAttribute();

        public string[] DependentProperties { get; set; }
        public object[] TargetValues { get; set; }

        public RequiredIfAnyAttribute(string[] dependentProperties, params object[] targetValues)
        {
            if (dependentProperties.Count() != targetValues.Count())
                throw new ArgumentException("Dependent properties count must equal values count");

            this.DependentProperties = dependentProperties;
            this.TargetValues = targetValues;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            for (int i = 0; i < DependentProperties.Count(); i++)
            {
                // get a reference to the property this validation depends upon
                var containerType = validationContext.ObjectInstance.GetType();
                var field = containerType.GetProperty(this.DependentProperties[i]);

                if (field != null)
                {
                    // get the value of the dependent property
                    var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);

                    // compare the value against the target value
                    if ((dependentvalue == null && this.TargetValues[i] == null) ||
                        (dependentvalue != null && dependentvalue.Equals(this.TargetValues[i])))
                    {
                        // match => means we should try validating this field
                        if (!_innerAttribute.IsValid(value))
                            // validation failed - return an error
                            return new ValidationResult(this.ErrorMessage, new[] { validationContext.MemberName });
                    }
                }
            }

            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule()
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "requiredifany",
            };

            var properties = new List<string>();
            var values = new List<object>();

            for (int i = 0; i < DependentProperties.Count(); i++)
            {
                string depProp = BuildDependentPropertyId(metadata, context as ViewContext, DependentProperties[i]);

                // find the value on the control we depend on;
                // if it's a bool, format it javascript style 
                // (the default is True or False!)
                string targetValue = (TargetValues[i] ?? "").ToString();
                if (TargetValues[i].GetType() == typeof(bool))
                    targetValue = targetValue.ToLower();

                properties.Add(depProp);
                values.Add(targetValue);
            }

            rule.ValidationParameters.Add("dependentproperties", String.Join("|", properties));
            rule.ValidationParameters.Add("targetvalues", String.Join("|", values));

            yield return rule;
        }

        private string BuildDependentPropertyId(ModelMetadata metadata, ViewContext viewContext, string dependentProperty)
        {
            // build the ID of the property
            string depProp = viewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(dependentProperty);
            // unfortunately this will have the name of the current field appended to the beginning,
            // because the TemplateInfo's context has had this fieldname appended to it. Instead, we
            // want to get the context as though it was one level higher (i.e. outside the current property,
            // which is the containing object (our Person), and hence the same level as the dependent property.
            var thisField = metadata.PropertyName + "_";
            if (depProp.StartsWith(thisField))
                // strip it off again
                depProp = depProp.Substring(thisField.Length);
            return depProp;
        }
    }
}