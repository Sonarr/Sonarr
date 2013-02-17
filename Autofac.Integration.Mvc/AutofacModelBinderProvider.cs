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
using System.Linq;
using System.Web.Mvc;
using Autofac.Features.Metadata;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Autofac implementation of the <see cref="IModelBinderProvider"/> interface.
    /// </summary>
    public class AutofacModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// Metadata key for the supported model types.
        /// </summary>
        internal static readonly string MetadataKey = "SupportedModelTypes";

        /// <summary>
        /// Gets the model binder associated with the provided model type.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns>An <see cref="IModelBinder"/> instance if found; otherwise, <c>null</c>.</returns>
        public IModelBinder GetBinder(Type modelType)
        {
            var modelBinders = DependencyResolver.Current.GetServices<Meta<Lazy<IModelBinder>>>();

            var modelBinder = modelBinders
                .Where(binder => binder.Metadata.ContainsKey(MetadataKey))
                .FirstOrDefault(binder => ((List<Type>)binder.Metadata[MetadataKey]).Contains(modelType));
            return (modelBinder != null) ? modelBinder.Value.Value : null;
        }
    }
}