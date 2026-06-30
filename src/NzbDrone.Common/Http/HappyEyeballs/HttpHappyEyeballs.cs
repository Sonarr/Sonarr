using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http.HappyEyeballs;

public class HttpHappyEyeballs
{
    private const int ConnectionAttemptDelay = 250;

    private readonly Logger _logger;

    public HttpHappyEyeballs(Logger logger)
    {
        _logger = logger;
    }

    public async ValueTask<Stream> OnConnect(SocketsHttpConnectionContext context, CancellationToken cancellationToken = default)
    {
        var endPoint = context.DnsEndPoint;
        var resolvedAddresses = await Dns.GetHostAddressesAsync(endPoint.Host, endPoint.AddressFamily, cancellationToken).ConfigureAwait(false);

        if (resolvedAddresses.Length == 0)
        {
            throw new WebException(
                $"The remote name {endPoint.Host} could not be resolved",
                WebExceptionStatus.NameResolutionFailure);
        }

        var happyEyeballs = CreateHappyEyeballs(endPoint);
        var socket = await happyEyeballs.Connect(resolvedAddresses, cancellationToken).ConfigureAwait(false);
        _logger.Trace("Successfully connected to {0} for host {1}", socket.RemoteEndPoint, endPoint.HostPort);

        return new NetworkStream(socket, ownsSocket: true);
    }

    private HappyEyeballs<Socket> CreateHappyEyeballs(DnsEndPoint endPoint)
    {
        return new HappyEyeballs<Socket>(
            (ipAddress, cancel) => ConnectSocket(ipAddress, endPoint, cancel),
            TaskDelay);
    }

    private async Task TaskDelay(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var timeSpan = TimeSpan.FromMilliseconds(ConnectionAttemptDelay);

        await Task.Delay(timeSpan, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Socket> ConnectSocket(IPAddress ipAddress, DnsEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        var remoteEndPoint = new IPEndPoint(ipAddress, endPoint.Port);

        // The following socket constructor will create a dual-mode socket on
        // systems where IPv6 is available.
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
        {
            // Turn off Nagle's algorithm since it degrades performance in most
            // HttpClient scenarios.
            NoDelay = true
        };

        _logger.Trace("Trying Happy Eyeballs connection to {0} for host {1}", ipAddress, endPoint.HostPort);

        try
        {
            await socket.ConnectAsync(remoteEndPoint, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            socket.Dispose();
            _logger.Trace("Failed Happy Eyeballs connection to {0} for host {1} ({2})", ipAddress, endPoint.HostPort, e.Message);
            throw;
        }

        return socket;
    }
}
