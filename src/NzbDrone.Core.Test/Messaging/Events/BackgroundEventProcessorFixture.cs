using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Messaging.Events
{
    [TestFixture]
    public class BackgroundEventProcessorFixture : TestBase<BackgroundEventProcessor>
    {
        [TearDown]
        public async Task Cleanup()
        {
            if (Subject != null)
            {
                try
                {
                    await Subject.DisposeAsync();
                }
                catch (ChannelClosedException)
                {
                }
            }
        }

        [Test]
        public async Task should_queue_and_process_background_event()
        {
            var testEvent = new TestBackgroundEvent();
            var handler = new TestBackgroundHandler();
            var tcs = new TaskCompletionSource<bool>();
            handler.OnHandled = () => tcs.TrySetResult(true);

            await Subject.QueueEventAsync(testEvent, handler);

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            completed.Should().Be(tcs.Task, "handler should have been called within timeout");
            handler.WasHandled.Should().BeTrue();
            handler.ReceivedEvent.Should().Be(testEvent);
        }

        [Test]
        public async Task should_handle_multiple_events_concurrently()
        {
            const int eventCount = 100;
            var handlers = new TestBackgroundHandler[eventCount];
            var tasks = new Task[eventCount];

            for (var i = 0; i < eventCount; i++)
            {
                handlers[i] = new TestBackgroundHandler();
                var tcs = new TaskCompletionSource<bool>();
                var handler = handlers[i];
                handler.OnHandled = () => tcs.TrySetResult(true);
                tasks[i] = tcs.Task;
            }

            for (var i = 0; i < eventCount; i++)
            {
                await Subject.QueueEventAsync(new TestBackgroundEvent(), handlers[i]);
            }

            var allTasksCompleted = Task.WhenAll(tasks);
            var timeoutTask = Task.Delay(10000);
            var completedTask = await Task.WhenAny(allTasksCompleted, timeoutTask);

            completedTask.Should().NotBe(timeoutTask, "all handlers should complete within timeout");

            foreach (var handler in handlers)
            {
                handler.WasHandled.Should().BeTrue();
            }
        }

        private class TestBackgroundEvent : IEvent
        {
        }

        private class TestBackgroundHandler : IHandleBackgroundAsync<IEvent>
        {
            public bool WasHandled { get; private set; }
            public IEvent ReceivedEvent { get; private set; }
            public Action OnHandled { get; set; }

            public Task HandleAsync(IEvent message, CancellationToken cancellationToken)
            {
                ReceivedEvent = message;
                WasHandled = true;
                OnHandled?.Invoke();
                return Task.CompletedTask;
            }
        }

        private class FaultyBackgroundHandler : IHandleBackgroundAsync<IEvent>
        {
            public Task HandleAsync(IEvent message, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Simulated handler failure");
            }
        }
    }
}
