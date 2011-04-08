using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AutoMoq.Unity;
using Microsoft.Practices.Unity;
using Moq;
using Moq.Language.Flow;

[assembly: InternalsVisibleTo("AutoMoq.Tests")]

namespace AutoMoq
{
    public class AutoMoqer
    {
        private IUnityContainer container;
        private IDictionary<Type, object> registeredMocks;
        internal Type ResolveType = null;

        public AutoMoqer()
        {
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
            ResolveType = null;
            return result;
        }

        public virtual Mock<T> GetMock<T>() where T : class
        {
            ResolveType = null;
            var type = GetTheMockType<T>();
            if (GetMockHasNotBeenCalledForThisType(type))
                CreateANewMockAndRegisterIt<T>(type);

            return TheRegisteredMockForThisType<T>(type);
        }

        internal virtual void SetMock(Type type, Mock mock)
        {
            if (registeredMocks.ContainsKey(type) == false)
                registeredMocks.Add(type, mock);
        }

        public virtual void SetConstant<T>(T instance) where T : class
        {
            container.RegisterInstance(instance);
            SetMock(instance.GetType(), null);
        }

        #region private methods

        private void SetupAutoMoqer(IUnityContainer container)
        {
            this.container = container;
            registeredMocks = new Dictionary<Type, object>();

            AddTheAutoMockingContainerExtensionToTheContainer(container);
            container.RegisterInstance(this);
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

        private void CreateANewMockAndRegisterIt<T>(Type type) where T : class
        {
            var mock = new Mock<T>();
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

    }
}