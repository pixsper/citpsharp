using System;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp
{
	/// <summary>
	///     Base interface for CITP devices which have server functionality
	/// </summary>
		public interface ICitpServerDevice : ICitpDevice
	{
		/// <summary>
		///     The unique identifier of this server device
		/// </summary>
		Guid Uuid { get; }

		/// <summary>
		///     Hashset of supported image formats for CITP peers making streaming requests from this server device
		/// </summary>
		IImmutableSet<MsexImageFormat> SupportedStreamFormats { get; }

		/// <summary>
		///     Dictionary containing information on available streaming video sources, where the key is the unique video source
		///     ID.
		/// </summary>
		IImmutableDictionary<int, VideoSourceInformation> VideoSourceInformation { get; }

		/// <summary>
		///     Requests a frame from a streaming video source
		/// </summary>
		/// <param name="sourceId">ID of the video source</param>
		/// <param name="request">Image request parameters</param>
		/// <returns>A <see cref="CitpImage" />, or null if the request was unsuccessful</returns>
		CitpImage? GetVideoSourceFrame(int sourceId, CitpImageRequest request);
	}
}