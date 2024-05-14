using System.Collections.Immutable;

namespace Pixsper.CitpSharp
{
	/// <summary>
	///     Interface allowing <see cref="CitpMediaServerService" /> access to properties of a host media server and ability to
	///     satisfy thumbnail/streaming requests.
	/// </summary>
		public interface ICitpMediaServerDevice : ICitpServerDevice
	{
		/// <summary>
		///     The name of the media server product
		/// </summary>
		string ProductName { get; }

		/// <summary>
		///     The major version of the media server product
		/// </summary>
		int ProductVersionMajor { get; }

		/// <summary>
		///     The minor version of the media server product
		/// </summary>
		int ProductVersionMinor { get; }

		/// <summary>
		///     The bugfix version of the media server product
		/// </summary>
		int ProductVersionBugfix { get; }

		/// <summary>
		///     An enumerable of MSEX versions supported by this media server
		/// </summary>
		IImmutableSet<MsexVersion> SupportedMsexVersions { get; }

		/// <summary>
		///     An enumerable of library types available on this media server
		/// </summary>
		IImmutableSet<MsexLibraryType> SupportedLibraryTypes { get; }

		/// <summary>
		///     An enumerable of supported image formats for CITP peers making thumbnail requests from this media server
		/// </summary>
		IImmutableSet<MsexImageFormat> SupportedThumbnailFormats { get; }

		/// <summary>
		///     An enumerable of available layers on this media server
		/// </summary>
		IImmutableList<ICitpMediaServerLayer> Layers { get; }

		/// <summary>
		///		Dictionary containing information on all element libraries
		/// </summary>
		IImmutableDictionary<MsexLibraryId, ElementLibrary> ElementLibraries { get; }

		/// <summary>
		///     Requests information from the media server on which libraries have been updated.
		/// </summary>
		/// <returns>An enumerable of library update information objects</returns>
		IImmutableList<ElementLibraryUpdatedInformation> GetLibraryUpdateInformation();


		/// <summary>
		///     Requests a library thumbnail from the media server
		/// </summary>
		/// <param name="request">Image request parameters to be used for requested thumbnail</param>
		/// <param name="elementLibrary">Library to request thumbnail for</param>
		/// <returns><see cref="CitpImage" /> for requested library or null if thumbnail is not available for request</returns>
        CitpImage? GetElementLibraryThumbnail(CitpImageRequest request, ElementLibraryInformation elementLibrary);

		/// <summary>
		///     Requests an element thumbnail from the media server
		/// </summary>
		/// <param name="request">Image request parameters to be used for requested thumbnail</param>
		/// <param name="elementLibrary">Information for library which contains element</param>
		/// <param name="element">Element to request thumbnail for</param>
		/// <returns><see cref="MsexLibraryId" /> and <see cref="CitpImage" /> for requested element</returns>
        CitpImage? GetElementThumbnail(CitpImageRequest request, ElementLibraryInformation elementLibrary, ElementInformation element);
	}



	/// <summary>
	///     Interface allowing <see cref="CitpMediaServerService" /> access to properties of individual layers on a host media
	///     server.
	/// </summary>
	/// <seealso cref="ICitpMediaServerDevice" />
    public interface ICitpMediaServerLayer
	{
		/// <summary>
		///     DMX patching information for this layer
		/// </summary>
		DmxPatchInfo DmxSource { get; }

		/// <summary>
		///     Zero-based index indicating the physical output on the media server this layer is linked to
		/// </summary>
		int PhysicalOutput { get; }

		/// <summary>
		///     The library type for elements which can be loaded to this layer (for MSEX 1.0)
		/// </summary>
		MsexLibraryType MediaLibraryType { get; }

		/// <summary>
		///     The index of the library containing elements which can be loaded to this layer (for MSEX 1.1+)
		/// </summary>
		int MediaLibraryIndex { get; }

		/// <summary>
		///     The ID of the library containing elements which can be loaded to this layer
		/// </summary>
		MsexLibraryId MediaLibraryId { get; }

		/// <summary>
		///     Index of the media loaded to this layer
		/// </summary>
		int MediaIndex { get; }

		/// <summary>
		///     Name of the media loaded to this layer
		/// </summary>
		string MediaName { get; }

		/// <summary>
		///     Current frame of the media loaded to this layer
		/// </summary>
		uint MediaFrame { get; }

		/// <summary>
		///     Frame count of the media loaded to this layer
		/// </summary>
		uint MediaNumFrames { get; }

		/// <summary>
		///     Frames per second of the media loaded to this layer
		/// </summary>
		int MediaFps { get; }

		/// <summary>
		///     Flags indicating media playback status information
		/// </summary>
		MsexLayerStatusFlags LayerStatusFlags { get; }
	}
}