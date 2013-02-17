// This software is part of the Autofac IoC container
// Copyright © 2012 Autofac Contributors
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
using System.ComponentModel;
using System.Reflection;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Metadata interface for filter registrations.
    /// </summary>
    internal class FilterMetadata
    {
        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        [DefaultValue(null)]
        public Type ControllerType { get; set; }

        /// <summary>
        /// Gets the filter scope.
        /// </summary>
        [DefaultValue(FilterScope.First)]
        public FilterScope FilterScope { get; set; }

        /// <summary>
        /// Gets the method info.
        /// </summary>
        [DefaultValue(null)]
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// Gets the order in which the filter is applied.
        /// </summary>
        [DefaultValue(-1)]
        public int Order { get; set; }
    }
}