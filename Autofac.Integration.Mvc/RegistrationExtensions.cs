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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Extends <see cref="ContainerBuilder"/> with methods to support ASP.NET MVC.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Share one instance of the component within the context of a single
        /// HTTP request.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerHttpRequest<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException("registration");

            return registration.InstancePerMatchingLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag);
        }

        /// <summary>
        /// Register types that implement IController in the provided assemblies.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="controllerAssemblies">Assemblies to scan for controllers.</param>
        /// <returns>Registration builder allowing the controller components to be customised.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterControllers(
                this ContainerBuilder builder,
                params Assembly[] controllerAssemblies)
        {
            return builder.RegisterAssemblyTypes(controllerAssemblies)
                .Where(t => typeof(IController).IsAssignableFrom(t) &&
                    t.Name.EndsWith("Controller", StringComparison.Ordinal));
        }

        /// <summary>
        /// Inject an IActionInvoker into the controller's ActionInvoker property.
        /// </summary>
        /// <typeparam name="TLimit">Limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registrationBuilder">The registration builder.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            InjectActionInvoker<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder)
        {
            return registrationBuilder.InjectActionInvoker(new TypedService(typeof(IActionInvoker)));
        }

        /// <summary>
        /// Inject an IActionInvoker into the controller's ActionInvoker property.
        /// </summary>
        /// <typeparam name="TLimit">Limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registrationBuilder">The registration builder.</param>
        /// <param name="actionInvokerService">Service used to resolve the action invoker.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            InjectActionInvoker<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder,
                Service actionInvokerService)
        {
            if (registrationBuilder == null) throw new ArgumentNullException("registrationBuilder");
            if (actionInvokerService == null) throw new ArgumentNullException("actionInvokerService");

            return registrationBuilder.OnActivating(e =>
            {
                var controller = e.Instance as Controller;
                if (controller != null)
                    controller.ActionInvoker = (IActionInvoker)e.Context.ResolveService(actionInvokerService);
            });
        }

        /// <summary>
        /// Registers the <see cref="AutofacModelBinderProvider"/>.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void RegisterModelBinderProvider(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            builder.RegisterType<AutofacModelBinderProvider>()
                .As<IModelBinderProvider>()
                .SingleInstance();
        }

        /// <summary>
        /// Sets a provided registration to act as an <see cref="System.Web.Mvc.IModelBinder"/>
        /// for the specified list of types.
        /// </summary>
        /// <param name="registration">
        /// The registration for the type or object instance that will act as
        /// the model binder.
        /// </param>
        /// <param name="types">
        /// The list of model <see cref="System.Type"/> for which the <paramref name="registration" />
        /// should be a model binder.
        /// </param>
        /// <typeparam name="TLimit">
        /// Registration limit type.
        /// </typeparam>
        /// <typeparam name="TActivatorData">
        /// Activator data type.
        /// </typeparam>
        /// <typeparam name="TRegistrationStyle">
        /// Registration style.
        /// </typeparam>
        /// <returns>
        /// An Autofac registration that can be modified as needed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="types" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="types" /> is empty or contains all <see langword="null" />
        /// values.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The declarative mechanism of registering model binders with Autofac
        /// is through use of <see cref="Autofac.Integration.Mvc.RegistrationExtensions.RegisterModelBinders"/>
        /// and the <see cref="Autofac.Integration.Mvc.ModelBinderTypeAttribute"/>.
        /// This method is an imperative alternative.
        /// </para>
        /// <para>
        /// The two mechanisms are mutually exclusive. If you register a model
        /// binder using <see cref="Autofac.Integration.Mvc.RegistrationExtensions.RegisterModelBinders"/>
        /// and register the same model binder with this method, the results
        /// are not automatically merged together - standard dependency
        /// registration/resolution rules will be used to manage the conflict.
        /// </para>
        /// <para>
        /// Any <see langword="null" /> values provided in <paramref name="types" />
        /// will be removed prior to registration.
        /// </para>
        /// </remarks>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AsModelBinderForTypes<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, params Type[] types)
            where TActivatorData : IConcreteActivatorData
            where TRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            var typeList = types.Where(type => type != null).ToList();
            if (typeList.Count == 0)
            {
                throw new ArgumentException(RegistrationExtensionsResources.InvalidModelBinderType, "types");
            }

            return registration.As<IModelBinder>().WithMetadata(AutofacModelBinderProvider.MetadataKey, typeList);
        }

        /// <summary>
        /// Register types that implement <see cref="IModelBinder"/> in the provided assemblies
        /// and have a <see cref="Autofac.Integration.Mvc.ModelBinderTypeAttribute"/>.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="modelBinderAssemblies">Assemblies to scan for model binders.</param>
        /// <returns>A registration builder.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> or <paramref name="modelBinderAssemblies" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The declarative mechanism of registering model binders with Autofac
        /// is through use of this method and the
        /// <see cref="Autofac.Integration.Mvc.ModelBinderTypeAttribute"/>.
        /// If you would like more imperative control over registration for your
        /// model binders, see the <see cref="AsModelBinderForTypes{TLimit,TActivatorData,TRegistrationStyle}"/>
        /// method.
        /// </para>
        /// <para>
        /// The two mechanisms are mutually exclusive. If you register a model
        /// binder using <see cref="AsModelBinderForTypes{TLimit,TActivatorData,TRegistrationStyle}"/>
        /// and register the same model binder with this method, the results
        /// are not automatically merged together - standard dependency
        /// registration/resolution rules will be used to manage the conflict.
        /// </para>
        /// <para>
        /// This method only registers types that implement <see cref="IModelBinder"/>
        /// and are marked with the <see cref="Autofac.Integration.Mvc.ModelBinderTypeAttribute"/>.
        /// The model binder must have the attribute because the
        /// <see cref="Autofac.Integration.Mvc.AutofacModelBinderProvider"/> uses
        /// the associated metadata - from the attribute(s) - to resolve the
        /// binder based on model type. If there aren't any attributes, there
        /// won't be any metadata, so the model binder will be technically
        /// registered but will never actually be resolved.
        /// </para>
        /// <para>
        /// If your model is not marked with the attribute, or if you don't want
        /// to use attributes, use the
        /// <see cref="AsModelBinderForTypes{TLimit,TActivatorData,TRegistrationStyle}"/>
        /// extension instead.
        /// </para>
        /// </remarks>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterModelBinders(this ContainerBuilder builder, params Assembly[] modelBinderAssemblies)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (modelBinderAssemblies == null) throw new ArgumentNullException("modelBinderAssemblies");

            return builder.RegisterAssemblyTypes(modelBinderAssemblies)
                .Where(type => typeof(IModelBinder).IsAssignableFrom(type) && type.GetCustomAttributes(typeof(ModelBinderTypeAttribute), true).Length > 0)
                .As<IModelBinder>()
                .InstancePerHttpRequest()
                .WithMetadata(AutofacModelBinderProvider.MetadataKey, type =>
                    (from ModelBinderTypeAttribute attribute in type.GetCustomAttributes(typeof(ModelBinderTypeAttribute), true)
                     from targetType in attribute.TargetTypes
                     select targetType).ToList());
        }

        /// <summary>
        /// Registers the <see cref="AutofacFilterProvider"/>.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void RegisterFilterProvider(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            foreach (var provider in FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().ToArray())
                FilterProviders.Providers.Remove(provider);

            builder.RegisterType<AutofacFilterProvider>()
                .As<IFilterProvider>()
                .SingleInstance();
        }

        /// <summary>
        /// Cache instances in the web session. This implies external ownership (disposal is not
        /// available.) All dependencies must also have external ownership.
        /// </summary>
        /// <remarks>
        /// It is strongly recommended that components cached per-session do not take dependencies on
        /// other services.
        /// </remarks>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "It is the responsibility of the registry to dispose of registrations.")]
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            CacheInSession<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
            where TActivatorData : IConcreteActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException("registration");

            var services = registration.RegistrationData.Services.ToArray();
            registration.RegistrationData.ClearServices();

            return registration
                .ExternallyOwned()
                .OnRegistered(e => e.ComponentRegistry.Register(RegistrationBuilder
                    .ForDelegate((c, p) =>
                    {
                        var session = HttpContext.Current.Session;
                        object result;
                        lock (session.SyncRoot)
                        {
                            result = session[e.ComponentRegistration.Id.ToString()];
                            if (result == null)
                            {
                                result = c.ResolveComponent(e.ComponentRegistration, p);
                                session[e.ComponentRegistration.Id.ToString()] = result;
                            }
                        }
                        return result;
                    })
                    .As(services)
                    .InstancePerLifetimeScope()
                    .ExternallyOwned()
                    .CreateRegistration()));
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IActionFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <param name="order">The order in which the filter is applied.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsActionFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector, int order = Filter.DefaultOrder) 
                    where TController : IController
        {
            return AsFilterFor<IActionFilter, TController>(registration, AutofacFilterProvider.ActionFilterMetadataKey, actionSelector, order);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IActionFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="order">The order in which the filter is applied.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsActionFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, int order = Filter.DefaultOrder) 
                where TController : IController
        {
            return AsFilterFor<IActionFilter, TController>(registration, AutofacFilterProvider.ActionFilterMetadataKey, order);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAuthorizationFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <param name="order">The order in which the filter is applied.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsAuthorizationFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector, int order = Filter.DefaultOrder) 
                    where TController : IController
        {
            return AsFilterFor<IAuthorizationFilter, TController>(registration, AutofacFilterProvider.AuthorizationFilterMetadataKey, actionSelector, order);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAuthorizationFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="order">The order in which the filter is applied.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsAuthorizationFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, int order = Filter.DefaultOrder) 
                where TController : IController
        {
            return AsFilterFor<IAuthorizationFilter, TController>(registration, AutofacFilterProvider.AuthorizationFilterMetadataKey, order);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IExceptionFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <param name="order">The order in which the filter is applied.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsExceptionFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector, int order = Filter.DefaultOrder) 
                    where TController : IController
        {
            return AsFilterFor<IExceptionFilter, TController>(registration, AutofacFilterProvider.ExceptionFilterMetadataKey, actionSelector, order);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IExceptionFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="order">The order in which the filter is applied.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsExceptionFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, int order = Filter.DefaultOrder) 
                where TController : IController
        {
            return AsFilterFor<IExceptionFilter, TController>(registration, AutofacFilterProvider.ExceptionFilterMetadataKey, order);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IResultFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <param name="order">The order in which the filter is applied.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsResultFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector, int order = Filter.DefaultOrder) 
                    where TController : IController
        {
            return AsFilterFor<IResultFilter, TController>(registration, AutofacFilterProvider.ResultFilterMetadataKey, actionSelector, order);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IResultFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="order">The order in which the filter is applied.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsResultFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, int order = Filter.DefaultOrder) 
                where TController : IController
        {
            return AsFilterFor<IResultFilter, TController>(registration, AutofacFilterProvider.ResultFilterMetadataKey, order);
        }

        static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsFilterFor<TFilter, TController>(IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, string metadataKey, Expression<Action<TController>> actionSelector, int order)
            where TController : IController
        {
            if (registration == null) throw new ArgumentNullException("registration");
            if (actionSelector == null) throw new ArgumentNullException("actionSelector");

            var limitType = registration.ActivatorData.Activator.LimitType;

            if (!limitType.IsAssignableTo<TFilter>())
            {
                var message = string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MustBeAssignableToFilterType, 
                    limitType.FullName, typeof(TFilter).FullName);
                throw new ArgumentException(message, "registration");
            }

            var metadata = new FilterMetadata
            {
                ControllerType = typeof(TController),
                FilterScope = FilterScope.Action,
                MethodInfo = GetMethodInfo(actionSelector),
                Order = order
            };

            return registration.As<TFilter>().WithMetadata(metadataKey, metadata);
        }

        static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsFilterFor<TFilter, TController>(IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, string metadataKey, int order)
            where TController : IController
        {
            if (registration == null) throw new ArgumentNullException("registration");

            var limitType = registration.ActivatorData.Activator.LimitType;

            if (!limitType.IsAssignableTo<TFilter>())
            {
                var message = string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MustBeAssignableToFilterType,
                    limitType.FullName, typeof(TFilter).FullName);
                throw new ArgumentException(message, "registration");
            }

            var metadata = new FilterMetadata
            {
                ControllerType = typeof(TController),
                FilterScope = FilterScope.Controller,
                MethodInfo = null,
                Order = order
            };

            return registration.As<TFilter>().WithMetadata(metadataKey, metadata);
        }

        static MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            var outermostExpression = expression.Body as MethodCallExpression;

            if (outermostExpression == null)
                throw new ArgumentException(RegistrationExtensionsResources.InvalidActionExpress);

            return outermostExpression.Method;
        }
    }
}
