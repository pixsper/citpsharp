using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Pixsper.CitpSharp.Networking;
using Pixsper.CitpSharp.Packets;
using Pixsper.CitpSharp.Packets.Pinf;

namespace Pixsper.CitpSharp
{
	/// <summary>
	///     Base class for CITP services implementing multicast UDP and CITP peer discovery services
	/// </summary>
		public abstract class CitpService : IDisposable
	{
		private  static readonly TimeSpan PeerLocationPacketInterval = TimeSpan.FromSeconds(1);

		private readonly ICitpDevice _device;
		private readonly RegularTimer _peerLocationTimer;
		private bool _isDisposed;

		/// <summary>
		///		Constructs base <see cref="CitpServerService"/>
		/// </summary>
		/// <param name="logger">Implementation of <see cref="ILogger"/></param>
		/// <param name="device">Implementation of <see cref="ICitpDevice"/> used to resolve requests from service</param>
		/// <param name="flags">Optional flags used to configure service behavior</param>
		/// <param name="localIp">Address of network interface to start network services on</param>
		protected CitpService(ILogger logger, ICitpDevice device, CitpServiceFlags flags, IPAddress? localIp = null)
		{
			Logger = logger;
			_device = device;
	        Flags = flags;

			UdpService = new UdpService(logger, Flags.HasFlag(CitpServiceFlags.UseLegacyMulticastIp), localIp);
			UdpService.PacketReceived += (s, e) => onUdpPacketReceived(e.Packet, e.Ip);

			_peerLocationTimer = new RegularTimer(PeerLocationPacketInterval);
			_peerLocationTimer.Elapsed += (s, e) => SendPeerLocationPacket();
			_peerLocationTimer.Start();
		}

		/// <summary>
		///		Type of CITP device
		/// </summary>
		public abstract CitpPeerType DeviceType { get; }

		/// <summary>
		///		Flags used to configure CITP service
		/// </summary>
		public CitpServiceFlags Flags { get; }

		/// <summary>
		///		Dispose implementation
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Dispose pattern implementation
		/// </summary>
		/// <param name="isDisposing"></param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (!_isDisposed)
			{
				if (isDisposing)
				{
					_peerLocationTimer.Dispose();
					UdpService.Dispose();
				}
			}

			_isDisposed = true;
		}


	    private protected ILogger Logger { get; }
        private protected PeerRegistry PeerRegistry { get; } = new PeerRegistry();
        private protected UdpService UdpService { get; }

		/// <summary>
		///		Sends peer location packet via UDP
		/// </summary>
		protected virtual void SendPeerLocationPacket()
		{
			UdpService.SendPacket(new PeerLocationPacket(false, 0, DeviceType, _device.PeerName, _device.State));
		}

		internal virtual void OnPinfPacketReceived(PinfPacket packet, IPAddress ip)
		{
			switch (packet.MessageType)
			{
				case PinfMessageType.PeerLocationMessage:
					Logger.LogDebug($"PINF Peer Location packet received from {ip}");
					PeerRegistry.AddPeer((PeerLocationPacket)packet, ip);
					break;

				case PinfMessageType.PeerNameMessage:
					Logger.LogDebug($"PINF Peer Name packet received from {ip}");
					PeerRegistry.AddPeer((PeerNamePacket)packet, ip);
					break;
			}
		}

		internal virtual void OnMsexPacketReceived(MsexPacket packet, IPAddress ip) { }



		private void onUdpPacketReceived(CitpPacket packet, IPAddress ip)
		{
			switch (packet.LayerType)
			{
				case CitpLayerType.PeerInformationLayer:
					OnPinfPacketReceived((PinfPacket)packet, ip);
					break;

				case CitpLayerType.MediaServerExtensionsLayer:
					OnMsexPacketReceived((MsexPacket)packet, ip);
					break;
			}
		}
	}
}