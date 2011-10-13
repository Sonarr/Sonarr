using FluentAssertions;
using NUnit.Framework;
using Ninject;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class CentralDispatchTests
    {
        [Test]
        public void Kernel_can_get_kernel()
        {
            CentralDispatch.Kernel.Should().NotBeNull();
        }

        [Test]
        public void Kernel_should_return_same_kernel()
        {
            var firstKernel = CentralDispatch.Kernel;
            var secondKernel = CentralDispatch.Kernel;

            firstKernel.Should().BeSameAs(secondKernel);
        }

        [Test]
        public void Kernel_should_be_able_to_resolve_ApplicationServer()
        {
            var appServer = CentralDispatch.Kernel.Get<ApplicationServer>();

            appServer.Should().NotBeNull();
        }

        [Test]
        public void Kernel_should_resolve_same_ApplicationServer_instance()
        {
            var appServer1 = CentralDispatch.Kernel.Get<ApplicationServer>();
            var appServer2 = CentralDispatch.Kernel.Get<ApplicationServer>();

            appServer1.Should().BeSameAs(appServer2);
        }

    }
}
