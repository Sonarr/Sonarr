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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Indicates what types a model binder supports.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ModelBinderTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets the target types.
        /// </summary>
        public IEnumerable<Type> TargetTypes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBinderTypeAttribute"/> class.
        /// </summary>
        /// <param name="targetTypes">The target types.</param>
        public ModelBinderTypeAttribute(params Type[] targetTypes)
        {
            if (targetTypes == null) throw new ArgumentNullException("targetTypes");
            TargetTypes = targetTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBinderTypeAttribute"/> class.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        public ModelBinderTypeAttribute(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            TargetTypes = new Type[] { targetType };
        }
    }
}
