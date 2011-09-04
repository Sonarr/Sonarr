using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Helpers
{
    public static class ValueExtension
    {
        public static object ValueFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var memberEx = expression.Body as MemberExpression;

            if (memberEx == null)
                throw new ArgumentException("Body not a member-expression.");

            string name = memberEx.Member.Name;
            var model = html.ViewData.Model;
            var value = model.GetType().GetProperty(name).GetValue(model, null);

            return value;
        }
    }
}