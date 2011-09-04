using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Helpers
{
    public static class DescriptionExtension
    {
        public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var memberEx = expression.Body as MemberExpression;

            if (memberEx == null)
                throw new ArgumentException("Body not a member-expression.");

            string name = memberEx.Member.Name;

            var attributes = TypeDescriptor.GetProperties(typeof(TModel))[name].Attributes;
            var desc = (DescriptionAttribute)attributes[typeof(DescriptionAttribute)];

            return new MvcHtmlString(desc.Description);
        }
    }
}