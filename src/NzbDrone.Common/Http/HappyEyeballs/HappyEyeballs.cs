using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NzbDrone.Common.Http.HappyEyeballs;

/*
Until .NET implements Happy Eyeballs natively, use third-party implementation from
https://slugcat.systems/post/24-06-16-ipv6-is-hard-happy-eyeballs-dotnet-httpclient/#the-implementation
This issue is being tracked at https://github.com/dotnet/runtime/issues/26177.

Below is a slightly modified Happy Eyeballs implementation from the post above.
We’ve factored out HTTP-specific implementation into HttpHappyEyeballs class to
make testing easier.
*/

public class HappyEyeballs<TSocket>
    where TSocket : IDisposable
{
    private readonly Func<IPAddress, CancellationToken, Task<TSocket>> _connectSocket;
    private readonly Func<CancellationToken, Task> _taskDelay;

    public HappyEyeballs(
        Func<IPAddress, CancellationToken, Task<TSocket>> connectSocket,
        Func<CancellationToken, Task> taskDelay)
    {
        _connectSocket = connectSocket;
        _taskDelay = taskDelay;
    }

    public async ValueTask<TSocket> Connect(
        IPAddress[] addresses,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(addresses.Length);

        var ips = SortInterleaved(addresses);

        return await ParallelTask(
            ips.Length,
            (i, cancel) => _connectSocket(ips[i], cancel),
            cancellationToken).ConfigureAwait(false);
    }

    private IPAddress[] SortInterleaved(IPAddress[] addresses)
    {
        // Interleave returned addresses so that they are IPv6 -> IPv4 -> IPv6 -> IPv4.
        // Assuming we have multiple addresses of the same type that is.
        // As described in the RFC.
        var ipv6 = addresses.Where(x => x.AddressFamily == AddressFamily.InterNetworkV6).ToArray();
        var ipv4 = addresses.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray();

        var commonLength = Math.Min(ipv6.Length, ipv4.Length);

        var result = new IPAddress[addresses.Length];

        for (var i = 0; i < commonLength; i++)
        {
            result[i * 2] = ipv6[i];
            result[1 + (i * 2)] = ipv4[i];
        }

        if (ipv4.Length > ipv6.Length)
        {
            ipv4.AsSpan(commonLength).CopyTo(result.AsSpan(commonLength * 2));
        }
        else if (ipv6.Length > ipv4.Length)
        {
            ipv6.AsSpan(commonLength).CopyTo(result.AsSpan(commonLength * 2));
        }

        return result;
    }

    private async Task<TSocket> ParallelTask(
        int totalTasks,
        Func<int, CancellationToken, Task<TSocket>> taskBuilder,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalTasks);

        using var successCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var taskIndex = 0;
        var tasks = new List<Task<TSocket>>();
        var innerExceptions = new List<Exception>();

        // The general loop here is as follows:
        // 1. Add a new task for the next IP to try.
        // 2. Wait until any task completes OR the delay happens.
        // If an error occurs, we stop checking that task and continue checking the next.
        // Every iteration we add another task, until we're full on them.
        // We keep looping until we have SUCCESS, or we run out of attempt tasks entirely.

        Task<TSocket> successTask = null;

        while (taskIndex < totalTasks || tasks.Count > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (taskIndex < totalTasks)
            {
                tasks.Add(taskBuilder(taskIndex, successCts.Token));
                taskIndex++;
            }

            var whenAnyDone = Task.WhenAny(tasks);
            Task<TSocket> completedTask;

            if (taskIndex < totalTasks)
            {
                using var delayCts = CancellationTokenSource.CreateLinkedTokenSource(successCts.Token);

                // If we have another one to queue, wait for a timeout instead of *just* waiting for a connection task.
                var timeoutTask = _taskDelay(delayCts.Token);
                var whenAnyOrTimeout = await Task.WhenAny(whenAnyDone, timeoutTask).ConfigureAwait(false);

                if (whenAnyOrTimeout != whenAnyDone)
                {
                    continue;
                }

                // Ensure that we dispose the internal timer associated with Task.Delay.
                await delayCts.CancelAsync().ConfigureAwait(false);
                await timeoutTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                completedTask = whenAnyDone.Result;
            }
            else
            {
                completedTask = await whenAnyDone.ConfigureAwait(false);
            }

            tasks.Remove(completedTask);

            if (completedTask.IsCompletedSuccessfully)
            {
                successTask = completedTask;
                break;
            }
            else if (completedTask.IsFaulted)
            {
                innerExceptions.AddRange(completedTask.Exception!.InnerExceptions);
            }
        }

        // Cancel and wait for all pending tasks.
        await successCts.CancelAsync().ConfigureAwait(false);
        await Task.WhenAll(tasks.Cast<Task>()).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        // Make sure that we don't get multiple sockets completing at once.
        //
        // Also for cancellation, e.g. if delay task completes before
        // socket connection in the task loop, and we receive cancellation
        // at the same time (thus stopping the loop). In this case, prefer
        // throwing an exception instead of returning a successful task.
        foreach (var task in tasks)
        {
            if (task.IsCompletedSuccessfully)
            {
                task.Result.Dispose();
            }
        }

        if (successTask == null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new AggregateException(innerExceptions);
        }

        return successTask.Result;
    }
}
