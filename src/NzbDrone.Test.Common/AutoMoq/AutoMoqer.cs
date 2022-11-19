using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DryIoc;
using Moq;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Test.Common.AutoMoq
{
    [DebuggerStepThrough]
    public class AutoMoqer
    {
        public readonly MockBehavior DefaultBehavior = MockBehavior.Default;
        private IContainer _container;
        private IDictionary<Type, object> _registeredMocks;

        public AutoMoqer()
        {
            SetupAutoMoqer(CreateTestContainer(new Container()));
        }

        public virtual T Resolve<T>()
        {
            var result = _container.Resolve<T>();
            SetConstant(result);
            return result;
        }

        public virtual Mock<T> GetMock<T>()
            where T : class
        {
            return GetMock<T>(DefaultBehavior);
        }

        public virtual Mock<T> GetMock<T>(MockBehavior behavior)
            where T : class
        {
            var type = GetTheMockType<T>();
            if (GetMockHasNotBeenCalledForThisType(type))
            {
                CreateANewMockAndRegisterIt<T>(type, behavior);
            }

            var mock = TheRegisteredMockForThisType<T>(type);

            if (behavior != MockBehavior.Default && mock.Behavior == MockBehavior.Default)
            {
                throw new InvalidOperationException("Unable to change be behaviour of a an existing mock.");
            }

            return mock;
        }

        public virtual void SetMock(Type type, Mock mock)
        {
            if (GetMockHasNotBeenCalledForThisType(type))
            {
                _registeredMocks.Add(type, mock);
            }

            if (mock != null)
            {
                _container.RegisterInstance(type, mock.Object, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            }
        }

        public virtual void SetConstant<T>(T instance)
        {
            _container.RegisterInstance(instance, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            SetMock(instance.GetType(), null);
        }

        private IContainer CreateTestContainer(IContainer container)
        {
            var c = container.CreateChild(IfAlreadyRegistered.Replace,
                container.Rules
                    .WithDynamicRegistration((serviceType, serviceKey) =>
                    {
                        // ignore services with non-default key
                        if (serviceKey != null)
                        {
                            return null;
                        }

                        if (serviceType == typeof(object))
                        {
                            return null;
                        }

                        if (serviceType.IsGenericType && serviceType.IsOpenGeneric())
                        {
                            return null;
                        }

                        if (serviceType == typeof(System.Text.Json.Serialization.JsonConverter))
                        {
                            return null;
                        }

                        // get the Mock object for the abstract class or interface
                        if (serviceType.IsInterface || serviceType.IsAbstract)
                        {
                            var mockType = typeof(Mock<>).MakeGenericType(serviceType);
                            var mockFactory = DelegateFactory.Of(r =>
                            {
                                var mock = (Mock)r.Resolve(mockType);
                                SetMock(serviceType, mock);
                                return mock.Object;
                            }, Reuse.Singleton);

                            return new[] { new DynamicRegistration(mockFactory, IfAlreadyRegistered.Keep) };
                        }

                        // concrete types
                        var concreteTypeFactory = serviceType.ToFactory(Reuse.Singleton, FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);

                        return new[] { new DynamicRegistration(concreteTypeFactory) };
                    },
                    DynamicRegistrationFlags.Service | DynamicRegistrationFlags.AsFallback));

            c.Register(typeof(Mock<>), Reuse.Singleton, FactoryMethod.DefaultConstructor());

            return c;
        }

        private void SetupAutoMoqer(IContainer container)
        {
            _container = container;
            container.RegisterInstance(this);

            _registeredMocks = new Dictionary<Type, object>();

            LoadPlatformLibrary();

            AssemblyLoader.RegisterNativeResolver(new[] { "System.Data.SQLite", "Sonarr.Core" });
        }

        private Mock<T> TheRegisteredMockForThisType<T>(Type type)
            where T : class
        {
            return (Mock<T>)_registeredMocks.First(x => x.Key == type).Value;
        }

        private void CreateANewMockAndRegisterIt<T>(Type type, MockBehavior behavior)
            where T : class
        {
            var mock = new Mock<T>(behavior);
            _container.RegisterInstance(mock.Object);
            SetMock(type, mock);
        }

        private bool GetMockHasNotBeenCalledForThisType(Type type)
        {
            return !_registeredMocks.ContainsKey(type);
        }

        private static Type GetTheMockType<T>()
            where T : class
        {
            return typeof(T);
        }

        private void LoadPlatformLibrary()
        {
            var assemblyName = "Sonarr.Windows";

            if (OsInfo.IsNotWindows)
            {
                assemblyName = "Sonarr.Mono";
            }

            if (!File.Exists(assemblyName + ".dll"))
            {
                return;
            }

            Assembly.Load(assemblyName);
        }
    }
}
