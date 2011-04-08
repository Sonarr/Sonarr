using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Moq;

namespace AutoMoq.Unity
{
    internal class AutoMockingBuilderStrategy : BuilderStrategy
    {
        private readonly MockFactory mockFactory;
        private readonly IEnumerable<Type> registeredTypes;
        private readonly IUnityContainer container;

        public AutoMockingBuilderStrategy(IEnumerable<Type> registeredTypes, IUnityContainer container)
        {
            mockFactory = new MockFactory(MockBehavior.Loose);
            this.registeredTypes = registeredTypes;
            this.container = container;
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            var autoMoqer = container.Resolve<AutoMoqer>();

            var type = GetTheTypeFromTheBuilderContext(context);
            if (AMockObjectShouldBeCreatedForThisType(type))
            {
                var mock = CreateAMockObject(type);
                context.Existing = mock.Object;
                autoMoqer.SetMock(type, mock);
            }
        }

        #region private methods

        private bool AMockObjectShouldBeCreatedForThisType(Type type)
        {
            var mocker = container.Resolve<AutoMoqer>();
            return TypeIsNotRegistered(type) && (mocker.ResolveType == null || mocker.ResolveType != type);
            //return TypeIsNotRegistered(type) && type.IsInterface;
        }

        private static Type GetTheTypeFromTheBuilderContext(IBuilderContext context)
        {
            return ((NamedTypeBuildKey)context.OriginalBuildKey).Type;
        }

        private bool TypeIsNotRegistered(Type type)
        {
            return registeredTypes.Any(x => x.Equals(type)) == false;
        }

        private Mock CreateAMockObject(Type type)
        {
            var createMethod = GenerateAnInterfaceMockCreationMethod(type);

            return InvokeTheMockCreationMethod(createMethod);
        }

        private Mock InvokeTheMockCreationMethod(MethodInfo createMethod)
        {
            return (Mock)createMethod.Invoke(mockFactory, new object[] { new List<object>().ToArray() });
        }

        private MethodInfo GenerateAnInterfaceMockCreationMethod(Type type)
        {
            var createMethodWithNoParameters = mockFactory.GetType().GetMethod("Create", EmptyArgumentList());

            return createMethodWithNoParameters.MakeGenericMethod(new[] { type });
        }

        private static Type[] EmptyArgumentList()
        {
            return new[] { typeof(object[]) };
        }

        #endregion
    }
}