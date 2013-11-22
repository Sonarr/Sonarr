// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Json;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class DefaultParameterResolver : IParameterResolver
    {
        /// <summary>
        /// Resolves a parameter value based on the provided object.
        /// </summary>
        /// <param name="descriptor">Parameter descriptor.</param>
        /// <param name="value">Value to resolve the parameter value from.</param>
        /// <returns>The parameter value.</returns>
        public virtual object ResolveParameter(ParameterDescriptor descriptor, IJsonValue value)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value.GetType() == descriptor.ParameterType)
            {
                return value;
            }

            return value.ConvertTo(descriptor.ParameterType);
        }

        /// <summary>
        /// Resolves method parameter values based on provided objects.
        /// </summary>
        /// <param name="method">Method descriptor.</param>
        /// <param name="values">List of values to resolve parameter values from.</param>
        /// <returns>Array of parameter values.</returns>
        public virtual IList<object> ResolveMethodParameters(MethodDescriptor method, IList<IJsonValue> values)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            return method.Parameters.Zip(values, ResolveParameter).ToArray();
        }
    }
}
