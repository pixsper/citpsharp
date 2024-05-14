using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pixsper.CitpSharp.Packets;

namespace Pixsper.CitpSharp.Networking
{
	internal sealed class UdpService : IDisposable
	{
	    const int CitpUdpPort = 4809;
	    static readonly IPAddress CitpMulticastIp = IPAddress.Parse("239.224.0.180");
	    static readonly IPAddress CitpMulticastLegacyIp = IPAddress.Parse("224.0.0.180");

	    private readonly ILogger _logger;

	    private bool _isDisposed;
	    private readonly UdpClient _client;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private readonly Task _listenTask;

	    public UdpService(ILogger logger, bool isUseLegacyMulticastIp, IPAddress? localIp = null)
	    {
		    _logger = logger;

			_logger.LogInformation("Starting UDP service...");

			MulticastIp = isUseLegacyMulticastIp ? CitpMulticastLegacyIp : CitpMulticastIp;

	        _client = new UdpClient
	        {
				EnableBroadcast = true,
	            MulticastLoopback = false
	        };

			_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_client.Client.Bind(new IPEndPoint(localIp ?? IPAddress.Any, CitpUdpPort));

			if (localIp != null)
				_client.JoinMulticastGroup(MulticastIp, localIp);
			else
				_client.JoinMulticastGroup(MulticastIp);

			_listenTask = listenAsync(_cancellationTokenSource.Token);

			_logger.LogInformation("Started UDP service");
		}


	    public void Dispose()
	    {
		    if (_isDisposed)
			    return;

			_logger.LogInformation("Stopping UDP service...");

			_cancellationTokenSource.Cancel();
			_listenTask.Wait();

			_cancellationTokenSource.Dispose();

			_client.Dispose();


		    _isDisposed = true;

			_logger.LogInformation("UDP service stopped");
		}


		public event EventHandler<CitpUdpPacketReceivedEventArgs>? PacketReceived;

		public IPAddress MulticastIp { get; }


		public void SendPacket(CitpPacket packet)
		{
			var buffer = packet.ToByteArray();

	        try
	        {
				var sendTask = _client.SendAsync(buffer, buffer.Length, new IPEndPoint(MulticastIp, CitpUdpPort));
				sendTask.Wait();

				if (sendTask.Result != buffer.Length)
					_logger.LogWarning("Failed to send UDP packet, {BytesSent}/{BufferLength} bytes sent", sendTask.Result, buffer.Length);
			}
	        catch (Exception ex)
	        {
				_logger.LogError(ex, "Exception whilst sending UDP CITP packet");
			}
		}

		private async Task listenAsync(CancellationToken ct)
		{
			var tcs = new TaskCompletionSource<bool>();

			using (ct.Register(s => tcs.TrySetResult(true), null))
			{
				try
				{
					while (!ct.IsCancellationRequested)
					{
						UdpReceiveResult result;

						try
						{
							var receiveTask = _client.ReceiveAsync();

							if (receiveTask != await Task.WhenAny(receiveTask, tcs.Task).ConfigureAwait(false))
								break;

							result = receiveTask.Result;
						}
						catch (SocketException ex)
						{
							_logger.LogError(ex, "Exception whilst receiving from UDP socket");
                            break;
						}
						catch (ObjectDisposedException)
						{
							break;
						}

						if (result.Buffer.Length < CitpPacket.MinimumPacketLength
						    || result.Buffer[0] != CitpPacket.CitpCookie[0]
						    || result.Buffer[1] != CitpPacket.CitpCookie[1]
						    || result.Buffer[2] != CitpPacket.CitpCookie[2]
						    || result.Buffer[3] != CitpPacket.CitpCookie[3])
						{
							_logger.LogInformation("Received non-CITP UDP packet");
							continue;
						}

						CitpPacket packet;

						try
						{
							packet = CitpPacket.FromByteArray(result.Buffer);
						}
						catch (InvalidOperationException ex)
						{
							_logger.LogWarning("Received malformed CITP packet: {Message}", ex.Message);
							continue;
						}
						catch (NotSupportedException ex)
						{
							_logger.LogWarning("Received unsupported CITP packet: {Message}", ex.Message);
							continue;
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Received unexpected exception type whilst deserializing CITP packet");
                            continue;
						}

						PacketReceived?.Invoke(this, new CitpUdpPacketReceivedEventArgs(packet, result.RemoteEndPoint.Address));
					}
				}
				finally
				{
					_client.DropMulticastGroup(MulticastIp);
				}
			}
		}
	}


	internal class CitpUdpPacketReceivedEventArgs : EventArgs
	{
		public CitpUdpPacketReceivedEventArgs(CitpPacket packet, IPAddress ip)
		{
			Packet = packet;
			Ip = ip;
		}

		public CitpPacket Packet { get; }
		public IPAddress Ip { get; }
	}
}
