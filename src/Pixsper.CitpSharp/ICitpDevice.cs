namespace Pixsper.CitpSharp
{
	/// <summary>
	///     Base interface for all CITP devices
	/// </summary>
		public interface ICitpDevice
	{
		/// <summary>
		///     The name of this CITP peer to be broadcast to other CITP peers
		/// </summary>
		string PeerName { get; }

		/// <summary>
		///     Text representation of the state of this device (eh. 'Online', 'Disabled', 'Reconfiguring', etc.)
		/// </summary>
		string State { get; }
	}
}