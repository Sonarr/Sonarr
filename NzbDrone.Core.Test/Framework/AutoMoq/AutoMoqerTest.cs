// ReSharper disable RedundantUsingDirective
using AutoMoq;
using Moq;
using System;
using NUnit.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class AutoMoqerTest
    {
        [Test]
        public void GetMock_on_interface_returns_mock()
        {
            //Arrange
            var mocker = new AutoMoqer();

            //Act
            var mock = mocker.GetMock<IDependency>();

            //Assert
            Assert.IsNotNull(mock);
        }

        [Test]
        public void GetMock_on_concrete_returns_mock()
        {
            //Arrange
            var mocker = new AutoMoqer();

            //Act
            var mock = mocker.GetMock<ConcreteClass>();

            //Assert
            Assert.IsNotNull(mock);
        }


        [Test]
        public void Resolve_doesnt_return_mock()
        {
            //Arrange
            var mocker = new AutoMoqer();

            //Act
            var result = mocker.Resolve<ConcreteClass>().Do();

            //Assert
            Assert.AreEqual("hello", result);
        }

        [Test]
        public void Resolve_with_dependency_doesnt_return_mock()
        {
            //Arrange
            var mocker = new AutoMoqer();

            //Act
            var result = mocker.Resolve<VirtualDependency>().VirtualMethod();

            //Assert
            Assert.AreEqual("hello", result);
        }

        [Test]
        public void Resolve_with_mocked_dependency_uses_mock()
        {
            //Arrange
            var mocker = new AutoMoqer();

            mocker.GetMock<VirtualDependency>()
                .Setup(m => m.VirtualMethod())
                .Returns("mocked");

            //Act
            var result = mocker.Resolve<ClassWithVirtualDependencies>().CallVirtualChild();

            //Assert
            Assert.AreEqual("mocked", result);
        }


        [Test]
        public void Resolve_with_unbound_concerete_dependency_uses_mock()
        {
            //Arrange
            var mocker = new AutoMoqer();

            //Act
            var result = mocker.Resolve<ClassWithVirtualDependencies>().CallVirtualChild();

            var mockedResult = new Mock<VirtualDependency>().Object.VirtualMethod();

            //Assert
            Assert.AreEqual(mockedResult, result);
        }


        [Test]
        public void Resolve_with_constant_concerete_dependency_uses_constant()
        {
            //Arrange
            var mocker = new AutoMoqer();

            var constant = new VirtualDependency {PropValue = Guid.NewGuid().ToString()};

            mocker.SetConstant(constant);

            //Act
            var result = mocker.Resolve<ClassWithVirtualDependencies>().GetVirtualProperty();

            //Assert
            Assert.AreEqual(constant.PropValue, result);
        }
    }

    public class ConcreteClass
    {
        public string Do()
        {
            return "hello";
        }
    }

    public class Dependency : IDependency
    {
    }

    public interface IDependency
    {
    }

    public class ClassWithDependencies
    {
        public ClassWithDependencies(IDependency dependency)
        {
            Dependency = dependency;
        }

        public IDependency Dependency { get; set; }
    }

    public class ClassWithVirtualDependencies
    {
        private readonly VirtualDependency _virtualDependency;

        public ClassWithVirtualDependencies(IDependency dependency, VirtualDependency virtualDependency)
        {
            _virtualDependency = virtualDependency;
            Dependency = dependency;
        }

        public IDependency Dependency { get; set; }

        public string CallVirtualChild()
        {
            return _virtualDependency.VirtualMethod();
        }

        public string GetVirtualProperty()
        {
            return _virtualDependency.PropValue;
        }
    }

    public class VirtualDependency
    {
        private readonly IDependency _dependency;

        public VirtualDependency()
        {
        }

        public VirtualDependency(IDependency dependency)
        {
            _dependency = dependency;
        }

        public string PropValue { get; set; }

        public virtual string VirtualMethod()
        {
            return "hello";
        }
    }
}