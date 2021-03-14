using System;

namespace Pixsper.CitpSharp
{
	/// <summary>
	///		Flags used to configure the runtime behavior of an instance of <see cref="CitpService"/>
	/// </summary>
		[Flags]
	public enum CitpServiceFlags
	{
		/// <summary>
		///		Null flag
		/// </summary>
		None = 0,
		/// <summary>
		///		Disables streaming frames, any requests for video sources from CITP peers will be passed an empty list
		/// </summary>
		DisableStreaming = 1 << 0,
		/// <summary>
		///		Disables library information, any requests for library information from CITP peers will be passed an empty list
		/// </summary>
		DisableLibraryInformation = 1 << 1,
		/// <summary>
		///		Disables element information, any requests for element information from CITP peers will be passed an empty list
		/// </summary>
		DisableElementInformation = 1 << 2,
		/// <summary>
		///		Disables library thumbnails, any requests for library thumbnails from CITP peers will be negatively acknowledged
		/// </summary>
		DisableLibraryThumbnails = 1 << 3,
		/// <summary>
		///		Disables element thumbnails, any requests for element thumbnails from CITP peers will be negatively acknowledged
		/// </summary>
		DisableElementThumbnails = 1 << 4,
		/// <summary>
		///		Disables regular transmission of the layer status packet
		/// </summary>
		DisableLayerStatus = 1 << 5,
		/// <summary>
		///		Disables the internal thread used to resolve streaming frame requests, requests must be resolved manually
		/// </summary>
		RunStreamThread = 1 << 6,
		/// <summary>
		///		Uses the CITP legacy multicast IP rather than the standard multicast IP
		/// </summary>
		UseLegacyMulticastIp = 1 << 7
	}
}
