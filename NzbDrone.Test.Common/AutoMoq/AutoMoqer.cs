// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Practices.Unity;
using Moq;
using Moq.Language.Flow;
using NzbDrone.Test.Common.AutoMoq.Unity;

[assembly: InternalsVisibleTo("AutoMoq.Tests")]

namespace NzbDrone.Test.Common.AutoMoq
{
    public class AutoMoqer
    {
        internal readonly MockBehavior DefaultBehavior = MockBehavior.Default;
        internal Type ResolveType;
        private IUnityContainer container;
        private IDictionary<Type, object> registeredMocks;

        public AutoMoqer()
        {
            SetupAutoMoqer(new UnityContainer());
        }

        public AutoMoqer(MockBehavior defaultBehavior)
        {
            DefaultBehavior = defaultBehavior;
            SetupAutoMoqer(new UnityContainer());

        }

        internal AutoMoqer(IUnityContainer container)
        {
            SetupAutoMoqer(container);
        }

        public virtual T Resolve<T>()
        {
            ResolveType = typeof(T);
            var result = container.Resolve<T>();
            SetConstant(result);
            ResolveType = null;
            return result;
        }

        public virtual Mock<T> GetMock<T>() where T : class
        {
            return GetMock<T>(DefaultBehavior);
        }

        public virtual Mock<T> GetMock<T>(MockBehavior behavior) where T : class
        {
            ResolveType = null;
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

        internal virtual void SetMock(Type type, Mock mock)
        {
            if (registeredMocks.ContainsKey(type) == false)
                registeredMocks.Add(type, mock);
        }

        public virtual void SetConstant<T>(T instance)
        {
            container.RegisterInstance(instance);
            SetMock(instance.GetType(), null);
        }

        public ISetup<T> Setup<T>(Expression<Action<T>> expression) where T : class
        {
            return GetMock<T>().Setup(expression);
        }

        public ISetup<T, TResult> Setup<T, TResult>(Expression<Func<T, TResult>> expression) where T : class
        {
            return GetMock<T>().Setup(expression);
        }

        public void Verify<T>(Expression<Action<T>> expression) where T : class
        {
            GetMock<T>().Verify(expression);
        }

        public void Verify<T>(Expression<Action<T>> expression, string failMessage) where T : class
        {
            GetMock<T>().Verify(expression, failMessage);
        }

        public void Verify<T>(Expression<Action<T>> expression, Times times) where T : class
        {
            GetMock<T>().Verify(expression, times);
        }

        public void Verify<T>(Expression<Action<T>> expression, Times times, string failMessage) where T : class
        {
            GetMock<T>().Verify(expression, times, failMessage);
        }

        public void VerifyAllMocks()
        {
            foreach (var registeredMock in registeredMocks)
            {
                var mock = registeredMock.Value as Mock;
                if (mock != null)
                    mock.VerifyAll();
            }
        }

        #region private methods

        private void SetupAutoMoqer(IUnityContainer container)
        {
            this.container = container;
            container.RegisterInstance(this);

            registeredMocks = new Dictionary<Type, object>();
            AddTheAutoMockingContainerExtensionToTheContainer(container);
        }

        private static void AddTheAutoMockingContainerExtensionToTheContainer(IUnityContainer container)
        {
            container.AddNewExtension<AutoMockingContainerExtension>();
            return;
        }

        private Mock<T> TheRegisteredMockForThisType<T>(Type type) where T : class
        {
            return (Mock<T>)registeredMocks.Where(x => x.Key == type).First().Value;
        }

        private void CreateANewMockAndRegisterIt<T>(Type type, MockBehavior behavior) where T : class
        {
            var mock = new Mock<T>(behavior);
            container.RegisterInstance(mock.Object);
            SetMock(type, mock);
        }

        private bool GetMockHasNotBeenCalledForThisType(Type type)
        {
            return registeredMocks.ContainsKey(type) == false;
        }

        private static Type GetTheMockType<T>() where T : class
        {
            return typeof(T);
        }

        #endregion
    }
}