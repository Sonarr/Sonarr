// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNet.SignalR.Hubs
{
    internal class HubMethodDispatcher
    {
        private HubMethodExecutor _executor;

        public HubMethodDispatcher(MethodInfo methodInfo)
        {
            _executor = GetExecutor(methodInfo);
            MethodInfo = methodInfo;
        }

        private delegate object HubMethodExecutor(IHub hub, object[] parameters);

        private delegate void VoidHubMethodExecutor(IHub hub, object[] parameters);

        public MethodInfo MethodInfo { get; private set; }

        public object Execute(IHub hub, object[] parameters)
        {
            return _executor(hub, parameters);
        }

        private static HubMethodExecutor GetExecutor(MethodInfo methodInfo)
        {
            // Parameters to executor
            ParameterExpression hubParameter = Expression.Parameter(typeof(IHub), "hub");
            ParameterExpression parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // Build parameter list
            List<Expression> parameters = new List<Expression>();
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                ParameterInfo paramInfo = paramInfos[i];
                BinaryExpression valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                UnaryExpression valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

                // valueCast is "(Ti) parameters[i]"
                parameters.Add(valueCast);
            }

            // Call method
            UnaryExpression instanceCast = (!methodInfo.IsStatic) ? Expression.Convert(hubParameter, methodInfo.ReflectedType) : null;
            MethodCallExpression methodCall = Expression.Call(instanceCast, methodInfo, parameters);

            // methodCall is "((TController) hub) method((T0) parameters[0], (T1) parameters[1], ...)"
            // Create function
            if (methodCall.Type == typeof(void))
            {
                Expression<VoidHubMethodExecutor> lambda = Expression.Lambda<VoidHubMethodExecutor>(methodCall, hubParameter, parametersParameter);
                VoidHubMethodExecutor voidExecutor = lambda.Compile();
                return WrapVoidAction(voidExecutor);
            }
            else
            {
                // must coerce methodCall to match HubMethodExecutor signature
                UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                Expression<HubMethodExecutor> lambda = Expression.Lambda<HubMethodExecutor>(castMethodCall, hubParameter, parametersParameter);
                return lambda.Compile();
            }
        }

        private static HubMethodExecutor WrapVoidAction(VoidHubMethodExecutor executor)
        {
            return delegate(IHub hub, object[] parameters)
            {
                executor(hub, parameters);
                return null;
            };
        }
    }
}
