using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http.HappyEyeballs;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class HappyEyeballsFixture : TestBase
    {
        private static IPAddress ipv6Address1 = IPAddress.Parse("2001:db8::1");
        private static IPAddress ipv6Address2 = IPAddress.Parse("2001:db8::2");
        private static IPAddress ipv4Address1 = IPAddress.Parse("192.0.2.1");
        private static IPAddress ipv4Address2 = IPAddress.Parse("192.0.2.2");

        private Mock<Func<IPAddress, CancellationToken, Task<IDisposable>>> _connectSocketMock;
        private Mock<Func<CancellationToken, Task>> _taskDelayMock;
        private HappyEyeballs<IDisposable> _happyEyeballs;

        [SetUp]
        public void SetUp()
        {
            _connectSocketMock = new Mock<Func<IPAddress, CancellationToken, Task<IDisposable>>>(MockBehavior.Strict);
            _taskDelayMock = new Mock<Func<CancellationToken, Task>>(MockBehavior.Strict);
            _happyEyeballs = new HappyEyeballs<IDisposable>(_connectSocketMock.Object, _taskDelayMock.Object);
        }

        [Test]
        public void should_throw_exception_when_no_ips_resolved()
        {
            var addresses = Array.Empty<IPAddress>();
            var cancellationToken = CancellationToken.None;

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await _happyEyeballs.Connect(addresses, cancellationToken));
        }

        [Test]
        public async Task should_connect_successfully_when_valid_addresses_are_provided()
        {
            var addresses = new[]
            {
                ipv4Address1,
                ipv6Address1,
            };

            var cancellationToken = CancellationToken.None;

            var sequence = new MockSequence();
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv6Address1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IDisposable>(MockBehavior.Strict));
            _taskDelayMock.InSequence(sequence)
                .Setup(x => x(It.IsAny<CancellationToken>()))
                .Returns(TaskFromCancellationToken);

            var result = await _happyEyeballs.Connect(addresses, cancellationToken);

            result.Should().NotBeNull();

            _connectSocketMock.Verify(x => x(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()), Times.Once);
            _taskDelayMock.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void should_throw_aggregateexception_when_no_addresses_successfully_connect()
        {
            var addresses = new[]
            {
                ipv4Address1,
                ipv4Address2,
                ipv6Address1,
                ipv6Address2,
            };

            var cancellationToken = CancellationToken.None;

            var sequence = new MockSequence();
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv6Address1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            _taskDelayMock.InSequence(sequence)
                .Setup(x => x(It.IsAny<CancellationToken>()))
                .Returns(TaskFromCancellationToken);
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv4Address1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            _taskDelayMock.InSequence(sequence)
                .Setup(x => x(It.IsAny<CancellationToken>()))
                .Returns(TaskFromCancellationToken);
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv6Address2, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            _taskDelayMock.InSequence(sequence)
                .Setup(x => x(It.IsAny<CancellationToken>()))
                .Returns(TaskFromCancellationToken);
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv4Address2, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            var ex = Assert.ThrowsAsync<AggregateException>(async () =>
                await _happyEyeballs.Connect(addresses, cancellationToken));

            ex.InnerExceptions.Should().HaveCount(4);

            _connectSocketMock.Verify(x => x(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
            _taskDelayMock.Verify(x => x(It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Test]
        public void should_throw_operationcanceledexception_when_canceled()
        {
            var addresses = new[]
            {
                ipv6Address1,
            };

            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            _connectSocketMock
                .Setup(x => x(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()))
                .Callback((IPAddress _, CancellationToken _) => cancellationTokenSource.Cancel())
                .ThrowsAsync(new Exception());

            var ex = Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _happyEyeballs.Connect(addresses, cancellationToken));

            ex.CancellationToken.IsCancellationRequested.Should().BeTrue();

            _connectSocketMock.Verify(x => x(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task should_connect_to_ipv4_when_ipv6_connection_fails()
        {
            var addresses = new[]
            {
                ipv4Address1,
                ipv6Address1,
            };

            var cancellationToken = CancellationToken.None;

            var sequence = new MockSequence();
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv6Address1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            _taskDelayMock.InSequence(sequence)
                .Setup(x => x(It.IsAny<CancellationToken>()))
                .Returns(TaskFromCancellationToken);
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv4Address1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IDisposable>(MockBehavior.Strict));

            var result = await _happyEyeballs.Connect(addresses, cancellationToken);

            result.Should().NotBeNull();

            _connectSocketMock.Verify(x => x(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _taskDelayMock.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task should_dispose_multiple_successful_connections()
        {
            var addresses = new[]
            {
                ipv4Address1,
                ipv6Address1,
                ipv6Address2,
            };

            var cancellationToken = CancellationToken.None;

            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);
            disposableMock.Setup(x => x.Dispose());

            var sequence = new MockSequence();
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv6Address1, It.IsAny<CancellationToken>()))
                .Returns((IPAddress _, CancellationToken cancel) => ReturnAfterCancellation(cancel, disposableMock.Object));
            _taskDelayMock.InSequence(sequence)
                .Setup(x => x(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv4Address1, It.IsAny<CancellationToken>()))
                .Returns((IPAddress _, CancellationToken cancel) => ReturnAfterCancellation(cancel, disposableMock.Object));
            _taskDelayMock.InSequence(sequence)
                .Setup(x => x(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _connectSocketMock.InSequence(sequence)
                .Setup(x => x(ipv6Address2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IDisposable>(MockBehavior.Strict));

            var result = await _happyEyeballs.Connect(addresses, cancellationToken);

            result.Should().NotBeNull();

            _connectSocketMock.Verify(x => x(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _taskDelayMock.Verify(x => x(It.IsAny<CancellationToken>()), Times.Exactly(2));
            disposableMock.Verify(x => x.Dispose(), Times.Exactly(2));
        }

        private static async Task<IDisposable> ReturnAfterCancellation(CancellationToken cancellationToken, IDisposable socket)
        {
            await TaskFromCancellationToken(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            return socket;
        }

        private static Task TaskFromCancellationToken(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource();
            cancellationToken.Register(() => tcs.TrySetCanceled());
            return tcs.Task;
        }
    }
}
