using System.Net;
using Microsoft.Extensions.Logging;

namespace Pixsper.CitpSharp
{
	/// <summary>
	///     Runs CITP services for a visualizer device.
	/// </summary>
	public class CitpVisualizerService : CitpServerService
	{
		private readonly ICitpVisualizerDevice _device;

		/// <summary>
		///		Constructs <see cref="CitpVisualizerService"/>
		/// </summary>
		/// <param name="logger">Implementation of <see cref="ILogger"/></param>
		/// <param name="device">Implementation of <see cref="ICitpVisualizerDevice"/> used to resolve requests from service</param>
		/// <param name="flags">Optional flags used to configure service behavior</param>
		/// <param name="preferredTcpListenPort">Service will attempt to start on this port if available, otherwise an available port will be used</param>
		/// <param name="localIp">Address of network interface to start network services on</param>
		public CitpVisualizerService(ILogger logger, ICitpVisualizerDevice device, CitpServiceFlags flags = CitpServiceFlags.None, 
			int preferredTcpListenPort = 0, IPAddress? localIp = null)
			: base(logger, device, flags, preferredTcpListenPort, localIp)
		{
			_device = device;
		}

		/// <summary>
		///		Type of CITP device
		/// </summary>
		public override CitpPeerType DeviceType => CitpPeerType.Visualizer;
	}
}