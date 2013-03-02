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

using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Dependency injection module that registers abstractions for common
    /// web application properties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This <see cref="Autofac.Module"/> is primarily used during
    /// application startup (in <c>Global.asax</c>) to register
    /// mappings from commonly referenced contextual application properties
    /// to their corresponding abstraction.
    /// </para>
    /// <para>
    /// The following mappings are made:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Common Construct</term>
    /// <description>Abstraction</description>
    /// </listheader>
    /// <item>
    /// <term><c>HttpContext.Current</c></term>
    /// <description><see cref="System.Web.HttpContextBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Application</c></term>
    /// <description><see cref="System.Web.HttpApplicationStateBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Request</c></term>
    /// <description><see cref="System.Web.HttpRequestBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Request.Browser</c></term>
    /// <description><see cref="System.Web.HttpBrowserCapabilitiesBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Request.Files</c></term>
    /// <description><see cref="System.Web.HttpFileCollectionBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Request.RequestContext</c></term>
    /// <description><see cref="System.Web.Routing.RequestContext"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Response</c></term>
    /// <description><see cref="System.Web.HttpResponseBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Response.Cache</c></term>
    /// <description><see cref="System.Web.HttpCachePolicyBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Server</c></term>
    /// <description><see cref="System.Web.HttpServerUtilityBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HttpContext.Current.Session</c></term>
    /// <description><see cref="System.Web.HttpSessionStateBase"/></description>
    /// </item>
    /// <item>
    /// <term><c>HostingEnvironment.VirtualPathProvider</c></term>
    /// <description><see cref="System.Web.Hosting.VirtualPathProvider"/></description>
    /// </item>
    /// </list>
    /// <para>
    /// In addition, the <see cref="System.Web.Mvc.UrlHelper"/> type is registered
    /// for construction based on the current <see cref="System.Web.Routing.RequestContext"/>.
    /// </para>
    /// <para>
    /// The lifetime for each of these items is one web request.
    /// </para>
    /// </remarks>
    public class AutofacWebTypesModule : Module
    {
        /// <summary>
        /// Registers web abstractions with dependency injection.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> in which registration
        /// should take place.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method registers mappings between common current context-related
        /// web constructs and their associated abstract counterparts. See
        /// <see cref="Autofac.Integration.Mvc.AutofacWebTypesModule"/> for the complete
        /// list of mappings that get registered.
        /// </para>
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new HttpContextWrapper(HttpContext.Current))
                .As<HttpContextBase>()
                .InstancePerHttpRequest();

            // HttpContext properties
            builder.Register(c => c.Resolve<HttpContextBase>().Request)
                .As<HttpRequestBase>()
                .InstancePerHttpRequest();

            builder.Register(c => c.Resolve<HttpContextBase>().Response)
                .As<HttpResponseBase>()
                .InstancePerHttpRequest();

            builder.Register(c => c.Resolve<HttpContextBase>().Server)
                .As<HttpServerUtilityBase>()
                .InstancePerHttpRequest();

            builder.Register(c => c.Resolve<HttpContextBase>().Session)
                .As<HttpSessionStateBase>()
                .InstancePerHttpRequest();

            builder.Register(c => c.Resolve<HttpContextBase>().Application)
                .As<HttpApplicationStateBase>()
                .InstancePerHttpRequest();

            // HttpRequest properties
            builder.Register(c => c.Resolve<HttpRequestBase>().Browser)
                .As<HttpBrowserCapabilitiesBase>()
                .InstancePerHttpRequest();

            builder.Register(c => c.Resolve<HttpRequestBase>().Files)
                .As<HttpFileCollectionBase>()
                .InstancePerHttpRequest();

            builder.Register(c => c.Resolve<HttpRequestBase>().RequestContext)
                .As<RequestContext>()
                .InstancePerHttpRequest();

            // HttpResponse properties
            builder.Register(c => c.Resolve<HttpResponseBase>().Cache)
                .As<HttpCachePolicyBase>()
                .InstancePerHttpRequest();

            // HostingEnvironment properties
            builder.Register(c => HostingEnvironment.VirtualPathProvider)
                .As<VirtualPathProvider>()
                .InstancePerHttpRequest();

            // MVC types
            builder.Register(c => new UrlHelper(c.Resolve<RequestContext>()))
                .As<UrlHelper>()
                .InstancePerHttpRequest();
        }
    }
}
