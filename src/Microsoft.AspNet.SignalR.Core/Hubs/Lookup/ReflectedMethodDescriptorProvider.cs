// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.SignalR.Json;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class ReflectedMethodDescriptorProvider : IMethodDescriptorProvider
    {
        private readonly ConcurrentDictionary<string, IDictionary<string, IEnumerable<MethodDescriptor>>> _methods;
        private readonly ConcurrentDictionary<string, MethodDescriptor> _executableMethods;

        public ReflectedMethodDescriptorProvider()
        {
            _methods = new ConcurrentDictionary<string, IDictionary<string, IEnumerable<MethodDescriptor>>>(StringComparer.OrdinalIgnoreCase);
            _executableMethods = new ConcurrentDictionary<string, MethodDescriptor>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<MethodDescriptor> GetMethods(HubDescriptor hub)
        {
            return FetchMethodsFor(hub)
                .SelectMany(kv => kv.Value)
                .ToList();
        }

        /// <summary>
        /// Retrieves an existing dictionary of all available methods for a given hub from cache.
        /// If cache entry does not exist - it is created automatically by BuildMethodCacheFor.
        /// </summary>
        /// <param name="hub"></param>
        /// <returns></returns>
        private IDictionary<string, IEnumerable<MethodDescriptor>> FetchMethodsFor(HubDescriptor hub)
        {
            return _methods.GetOrAdd(
                hub.Name,
                key => BuildMethodCacheFor(hub));
        }

        /// <summary>
        /// Builds a dictionary of all possible methods on a given hub.
        /// Single entry contains a collection of available overloads for a given method name (key).
        /// This dictionary is being cached afterwards.
        /// </summary>
        /// <param name="hub">Hub to build cache for</param>
        /// <returns>Dictionary of available methods</returns>
        private static IDictionary<string, IEnumerable<MethodDescriptor>> BuildMethodCacheFor(HubDescriptor hub)
        {
            return ReflectionHelper.GetExportedHubMethods(hub.HubType)
                .GroupBy(GetMethodName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key,
                              group => group.Select(oload =>
                                  new MethodDescriptor
                                  {
                                      ReturnType = oload.ReturnType,
                                      Name = group.Key,
                                      NameSpecified = (GetMethodAttributeName(oload) != null),
                                      Invoker = new HubMethodDispatcher(oload).Execute,
                                      Hub = hub,
                                      Attributes = oload.GetCustomAttributes(typeof(Attribute), inherit: true).Cast<Attribute>(),
                                      Parameters = oload.GetParameters()
                                          .Select(p => new ParameterDescriptor
                                              {
                                                  Name = p.Name,
                                                  ParameterType = p.ParameterType,
                                              })
                                          .ToList()
                                  }),
                              StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Searches the specified <paramref name="hub">Hub</paramref> for the specified <paramref name="method"/>.
        /// </summary>
        /// <remarks>
        /// In the case that there are multiple overloads of the specified <paramref name="method"/>, the <paramref name="parameters">parameter set</paramref> helps determine exactly which instance of the overload should be resolved. 
        /// If there are multiple overloads found with the same number of matching parameters, none of the methods will be returned because it is not possible to determine which overload of the method was intended to be resolved.
        /// </remarks>
        /// <param name="hub">Hub to search for the specified <paramref name="method"/> on.</param>
        /// <param name="method">The method name to search for.</param>
        /// <param name="descriptor">If successful, the <see cref="MethodDescriptor"/> that was resolved.</param>
        /// <param name="parameters">The set of parameters that will be used to help locate a specific overload of the specified <paramref name="method"/>.</param>
        /// <returns>True if the method matching the name/parameter set is found on the hub, otherwise false.</returns>
        public bool TryGetMethod(HubDescriptor hub, string method, out MethodDescriptor descriptor, IList<IJsonValue> parameters)
        {
            string hubMethodKey = BuildHubExecutableMethodCacheKey(hub, method, parameters);

            if (!_executableMethods.TryGetValue(hubMethodKey, out descriptor))
            {
                IEnumerable<MethodDescriptor> overloads;

                if (FetchMethodsFor(hub).TryGetValue(method, out overloads))
                {
                    var matches = overloads.Where(o => o.Matches(parameters)).ToList();

                    // If only one match is found, that is the "executable" version, otherwise none of the methods can be returned because we don't know which one was actually being targeted
                    descriptor = matches.Count == 1 ? matches[0] : null;
                }
                else
                {
                    descriptor = null;
                }

                // If an executable method was found, cache it for future lookups (NOTE: we don't cache null instances because it could be a surface area for DoS attack by supplying random method names to flood the cache)
                if (descriptor != null)
                {
                    _executableMethods.TryAdd(hubMethodKey, descriptor);
                }
            }

            return descriptor != null;
        }

        private static string BuildHubExecutableMethodCacheKey(HubDescriptor hub, string method, IList<IJsonValue> parameters)
        {
            string normalizedParameterCountKeyPart;

            if (parameters != null)
            {
                normalizedParameterCountKeyPart = parameters.Count.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                // NOTE: we normalize a null parameter array to be the same as an empty (i.e. Length == 0) parameter array
                normalizedParameterCountKeyPart = "0";
            }

            // NOTE: we always normalize to all uppercase since method names are case insensitive and could theoretically come in diff. variations per call
            string normalizedMethodName = method.ToUpperInvariant();

            string methodKey = hub.Name + "::" + normalizedMethodName + "(" + normalizedParameterCountKeyPart + ")";

            return methodKey;
        }

        private static string GetMethodName(MethodInfo method)
        {
            return GetMethodAttributeName(method) ?? method.Name;
        }

        private static string GetMethodAttributeName(MethodInfo method)
        {
            return ReflectionHelper.GetAttributeValue<HubMethodNameAttribute, string>(method, a => a.MethodName);
        }
    }
}
