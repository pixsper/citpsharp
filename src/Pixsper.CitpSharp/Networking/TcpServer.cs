using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pixsper.CitpSharp.Packets;

namespace Pixsper.CitpSharp.Networking
{
	internal sealed class TcpServer : IDisposable
	{
		private bool _isDisposed;

		private readonly ILogger _logger;
		private readonly TcpListener _tcpListener;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _listenTask;

		private ImmutableHashSet<TcpClientWrapper> _clients = ImmutableHashSet<TcpClientWrapper>.Empty;

	    public static bool IsTcpPortAvailable(int port)
			=> IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().All(e => e.Port != port);

		public TcpServer(ILogger logger, IPEndPoint localEndPoint)
		{
			_logger = logger;
			_tcpListener = new TcpListener(localEndPoint);

			_tcpListener.Start();
	        ListenPort = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;

            _listenTask = listen(_cancellationTokenSource.Token);
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_cancellationTokenSource.Cancel();
            _listenTask.Wait();

			_cancellationTokenSource.Dispose();

			_isDisposed = true;
		}

		public event EventHandler<TcpServerConnection>? ConnectionOpened;
        public event EventHandler<TcpPacketReceivedEventArgs>? PacketReceived;
        public event EventHandler<TcpServerConnection>? ConnectionClosed;

	    public int ListenPort { get; }

	    public IEnumerable<TcpServerConnection> ConnectedClients => _clients.Select(c => c.ServerConnection);

		private async Task listen(CancellationToken ct)
		{
			try
			{
				while (!ct.IsCancellationRequested)
				{
					try
					{
						var client = await  _tcpListener.AcceptTcpClientAsync().WithCancellation(ct).ConfigureAwait(false);

						var connection = new TcpServerConnection(_logger, client);

                        connection.ConnectionOpened += (s, e) => ConnectionOpened?.Invoke(s, e);
						connection.ConnectionClosed += (s, e) => ConnectionClosed?.Invoke(s, e);
						connection.ConnectionClosed += onClientWrapperClosed;

						connection.PacketReceived += (s, e) => PacketReceived?.Invoke(s, e);

                        var clientTask = connection.ListenAsync(ct);

                        _clients = _clients.Add(new TcpClientWrapper(connection, clientTask));

                    }
					catch (SocketException ex)
					{
						_logger.LogError(ex, "Socket exception in TCP server");
                    }
					catch (OperationCanceledException){ }
                }
			}
			finally
            {
                await Task.WhenAll(_clients.Select(c => c.ListenTask)).ConfigureAwait(false);

				_tcpListener.Stop();
			}
		}

		private void onClientWrapperClosed(object sender, TcpServerConnection e)
        {
            var client = _clients.FirstOrDefault(c => c.ServerConnection == e);

			Debug.Assert(client != null);
            if (client != null)
                _clients = _clients.Remove(client);
        }
	}


    internal class TcpClientWrapper
    {
        public TcpClientWrapper(TcpServerConnection serverConnection, Task listenTask)
        {
            ServerConnection = serverConnection;
            ListenTask = listenTask;
        }

        public TcpServerConnection ServerConnection { get; }
		public Task ListenTask { get; }
    }

	internal class TcpPacketReceivedEventArgs : EventArgs
	{
		public TcpPacketReceivedEventArgs(CitpPacket packet, TcpServerConnection client)
		{
			Packet = packet;
			Client = client;
		}

		public CitpPacket Packet { get; }
		public TcpServerConnection Client { get; }
	}
}
