using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using NLog;

namespace NzbDrone.Core.Instrumentation
{
    public class LogInjectionModule : Module
    {
        protected override void AttachToComponentRegistration(IComponentRegistry registry, IComponentRegistration registration)
        {
            registration.Preparing += OnComponentPreparing;
        }

        static void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            e.Parameters = e.Parameters.Union(new[] 
                { 
                        new ResolvedParameter((p, i) => p.ParameterType == typeof(Logger), (p,i)=> GetLogger(p.Member.DeclaringType)) 
                });
        }
        
        private static object GetLogger(Type type)
        {
            return LogManager.GetLogger(type.Name);
        }
    }
}