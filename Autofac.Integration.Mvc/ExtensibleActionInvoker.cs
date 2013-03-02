// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Injects services from the container into the ASP.NET MVC invocation pipeline.
    /// This is a Async Controller Action Invoker which can be used for both async and non-async scenarios
    /// </summary>
    /// <remarks>
    /// <para>
    /// Action methods can include parameters that will be resolved from the
    /// container, along with regular parameters.
    /// </para>
    /// </remarks>
    public class ExtensibleActionInvoker : System.Web.Mvc.Async.AsyncControllerActionInvoker
    {
        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param><param name="parameterDescriptor">The parameter descriptor.</param>
        /// <returns>
        /// The parameter value.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="parameterDescriptor" /> is <see langword="null" />.
        /// </exception>
        protected override object GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
        {
            if (parameterDescriptor == null)
            {
                throw new ArgumentNullException("parameterDescriptor");
            }

            // Only resolve parameter values if injection is enabled.
            var context = AutofacDependencyResolver.Current.RequestLifetimeScope;
            var value = context.ResolveOptional(parameterDescriptor.ParameterType);

            if (value == null)
            {
                // Issue #368
                // If injection is enabled and the value can't be resolved, fall back to
                // the default behavior. Or if injection isn't enabled, fall back.
                // Unfortunately we can't do much to pre-determine if model binding will succeed
                // because model binding "knows" about a lot of stuff like arrays, certain generic
                // collection types, type converters, and other stuff that may or may not fail.
                try
                {
                    value = base.GetParameterValue(controllerContext, parameterDescriptor);
                }
                catch (MissingMethodException)
                {
                    // Don't do anything - this means the default model binder couldn't
                    // activate a new instance or figure out some other way to model
                    // bind it.
                }
            }

            return value;
        }
    }
}