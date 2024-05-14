using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
// ReSharper disable UnusedMember.Global

namespace Pixsper.CitpSharp
{
	internal class CitpId : Attribute
	{
		public CitpId(string id)
		{
			Id = Encoding.UTF8.GetBytes(id.Substring(0, 4));
		}

		public byte[] Id { get; }

		public string IdString => Encoding.UTF8.GetString(Id, 0, Id.Length);
	}



	internal static class CitpEnumHelper
	{
		private static readonly Dictionary<Type, Dictionary<string, Enum>> CitpIdMaps =
			new Dictionary<Type, Dictionary<string, Enum>>();

		/// <summary>
		///     Gets an attribute on an enum field value
		/// </summary>
		/// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
		/// <param name="enumVal">The enum value</param>
		/// <returns>The attribute of type T that exists on the enum value</returns>
		public static T GetCustomAttribute<T>(this Enum enumVal) where T : Attribute
		{
			return enumVal
				.GetType()
				.GetRuntimeField(enumVal.ToString())
				.GetCustomAttribute<T>(false);
		}

		public static T GetEnumFromIdString<T>(string s) where T : struct
		{
			var typeT = typeof(T);

            if (CitpIdMaps.TryGetValue(typeT, out var map))
				return (T)(object)map[s];

			var values = Enum.GetValues(typeT).Cast<Enum>();

			map = values.ToDictionary(v => v.GetCustomAttribute<CitpId>().IdString);

			CitpIdMaps.Add(typeT, map);

			return (T)(object)map[s];
		}
	}



	internal enum CitpLayerType : uint
	{
		[CitpId("PINF")] PeerInformationLayer,
		[CitpId("SDMX")] SendDmxLayer,
		[CitpId("FPTC")] FixturePatchLayer,
		[CitpId("FSEL")] FixtureSelectionLayer,
		[CitpId("FINF")] FixtureInformationLayer,
		[CitpId("MSEX")] MediaServerExtensionsLayer
	}



	internal enum PinfMessageType : uint
	{
		[CitpId("PNam")] PeerNameMessage,
		[CitpId("PLoc")] PeerLocationMessage
	}



	internal enum SdmxMessageType : uint
	{
		[CitpId("Capa")] CapabilitiesMessage,
		[CitpId("UNam")] UniverseNameMessage,
		[CitpId("EnId")] EncryptionIdentifierMessage,
		[CitpId("ChBk")] ChannelBlockMessage,
		[CitpId("ChLs")] ChannelListMessage,
		[CitpId("SXSr")] SetExternalSourceMessage,
		[CitpId("SXUS")] SetExternalUniverseSourceMessage
	}



	internal enum FptcMessageType : uint
	{
		[CitpId("Ptch")] PatchMessage,
		[CitpId("UPtc")] UnpatchMessage,
		[CitpId("SPtc")] SendPatchMessage
	}



	internal enum FselMessageType : uint
	{
		[CitpId("Sele")] SelectMessage,
		[CitpId("DeSe")] DeselectMessage
	}



	internal enum FinfMessageType : uint
	{
		[CitpId("SFra")] SendFramesMessage,
		[CitpId("Fram")] FramesMessage,
		[CitpId("SPos")] SendPositionMessage,
		[CitpId("Posi")] PositionMessage,
		[CitpId("LSta")] LiveStatusMessage
	}



	internal enum MsexMessageType : uint
	{
		[CitpId("CInf")] ClientInformationMessage,
		[CitpId("SInf")] ServerInformationMessage,
		[CitpId("Nack")] NegativeAcknowledgeMessage,
		[CitpId("LSta")] LayerStatusMessage,
		[CitpId("GELI")] GetElementLibraryInformationMessage,
		[CitpId("ELIn")] ElementLibraryInformationMessage,
		[CitpId("ELUp")] ElementLibraryUpdatedMessage,
		[CitpId("GEIn")] GetElementInformationMessage,
		[CitpId("MEIn")] MediaElementInformationMessage,
		[CitpId("EEIn")] EffectElementInformationMessage,
		[CitpId("GLEI")] GenericElementInformationMessage,
		[CitpId("GELT")] GetElementLibraryThumbnailMessage,
		[CitpId("ELTh")] ElementLibraryThumbnailMessage,
		[CitpId("GETh")] GetElementThumbnailMessage,
		[CitpId("EThn")] ElementThumbnailMessage,
		[CitpId("GVSr")] GetVideoSourcesMessage,
		[CitpId("VSrc")] VideoSourcesMessage,
		[CitpId("RqSt")] RequestStreamMessage,
		[CitpId("StFr")] StreamFrameMessage
	}



	/// <summary>
	///     Represents device type of a CITP peer
	/// </summary>
	public enum CitpPeerType
	{
		/// <summary>
		///     Lighting console peer
		/// </summary>
		LightingConsole,

		/// <summary>
		///     Media server peer
		/// </summary>
		MediaServer,

		/// <summary>
		///     Visualizer peer
		/// </summary>
		Visualizer,

		/// <summary>
		///     Operation hub peer
		/// </summary>
		OperationHub,

		/// <summary>
		///     Unknown peer type
		/// </summary>
		Unknown
	}



	internal enum SdmxCapability : ushort
	{
		ChannelList = 1,
		ExternalSource = 2,
		PerUniverseExternalSources = 3,
		ArtNetExternalSources = 101,
		Bsre131ExternalSources = 102,
		EtcNet2ExternalSources = 103,
		MaNetExternalSources = 104
	}



	/// <summary>
	///     Protocol versions of the CITP MSEX layer
	/// </summary>
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum MsexVersion : ushort
	{
		/// <summary>
		///     Unknown or unsupported MSEX version
		/// </summary>
		UnsupportedVersion = 0,

		/// <summary>
		///     MSEX V1.0
		/// </summary>
		Version1_0,

		/// <summary>
		///     MSEX V1.1
		/// </summary>
		Version1_1,

		/// <summary>
		///     MSEX V1.2
		/// </summary>
		Version1_2
	}



	/// <summary>
	///     Type of a CITP MSEX library
	/// </summary>
	public enum MsexLibraryType : byte
	{
		/// <summary>
		///     No flags
		/// </summary>
		None = 0,

		/// <summary>
		///     Media library
		/// </summary>
		Media = 1,

		/// <summary>
		///     Effects library
		/// </summary>
		Effects = 2,

		/// <summary>
		///     Cue library
		/// </summary>
		Cues = 3,

		/// <summary>
		///     Crossfade library
		/// </summary>
		Crossfades = 4,

		/// <summary>
		///     Mask library
		/// </summary>
		Mask = 5,

		/// <summary>
		///     Blend preset library
		/// </summary>
		BlendPresets = 6,

		/// <summary>
		///     Effect preset library
		/// </summary>
		EffectPresets = 7,

		/// <summary>
		///     Image preset library
		/// </summary>
		ImagePresets = 8,

		/// <summary>
		///     Mesh library
		/// </summary>
		Meshes = 9
	}



	/// <summary>
	///     Image format used by MSEX thumbnail or streaming frame
	/// </summary>
	/// <remarks>
	///     <see cref="MsexImageFormat.Rgb8" /> format is transmitted as BGR order in <see cref="MsexVersion.Version1_0" />.
	///     Fragmented formats (<see cref="MsexImageFormat.FragmentedJpeg" /> and <see cref="MsexImageFormat.FragmentedPng" />)
	///     are only supported for streaming frames.
	/// </remarks>
	public enum MsexImageFormat : uint
	{
		/// <summary>
		///     8-bit RGB format (BGR order for MSEX V1.0)
		/// </summary>
		[CitpId("RGB8")] Rgb8,

		/// <summary>
		///     PNG image format
		/// </summary>
		[CitpId("PNG ")] Png,

		/// <summary>
		///     JPEG image format
		/// </summary>
		[CitpId("JPEG")] Jpeg,

		/// <summary>
		///     Fragmented JPEG format
		/// </summary>
		[CitpId("fJPG")] FragmentedJpeg,

		/// <summary>
		///     Fragmented PNG format
		/// </summary>
		[CitpId("fPNG")] FragmentedPng
	}



	/// <summary>
	///     Flags containing additional information on an MSEX layer
	/// </summary>
	[Flags]
	public enum MsexLayerStatusFlags : uint
	{
		/// <summary>
		///     No flags
		/// </summary>
		None = 0x0000,

		/// <summary>
		///     Media is playing
		/// </summary>
		MediaPlaying = 0x0001,

		/// <summary>
		///     Media is playing in reverse
		/// </summary>
		MediaPlaybackReverse = 0x0002,

		/// <summary>
		///     Media playback is looped
		/// </summary>
		MediaPlaybackLooping = 0x0004,

		/// <summary>
		///     Media playback is in bounce mode (play forward then backwards)
		/// </summary>
		MediaPlaybackBouncing = 0x0008,

		/// <summary>
		///     Media playback is in random frame mode
		/// </summary>
		MediaPlaybackRandom = 0x0010,

		/// <summary>
		///     Media playback is paused
		/// </summary>
		MediaPaused = 0x0020
	}



	/// <summary>
	///     Flags containing additional information for a MSEX library update notification
	/// </summary>
	[Flags]
	public enum MsexElementLibraryUpdatedFlags : byte
	{
		/// <summary>
		///     No flags
		/// </summary>
		None = 0x00,

		/// <summary>
		///     Existing elements in this library have been updated
		/// </summary>
		ExistingElementsUpdated = 0x01,

		/// <summary>
		///     Elements have been added or removed from this library
		/// </summary>
		ElementsAddedOrRemoved = 0x02,

		/// <summary>
		///     Sub-libraries of this library have been updated
		/// </summary>
		SubLibrariesUpdated = 0x04,

		/// <summary>
		///     Sub-libraries of this library have been added or removed
		/// </summary>
		SubLibrariesAddedOrRemoved = 0x08
	}



	/// <summary>
	///     Flags containing additional information when sending a library/element thumbnail
	/// </summary>
	[Flags]
	public enum MsexThumbnailFlags : byte
	{
		/// <summary>
		///     No flags
		/// </summary>
		None = 0x00,

		/// <summary>
		///     Thumbnails should be returned without distorting the aspect ratio
		/// </summary>
		PreserveAspectRatio = 0x01
	}



	/// <summary>
	///     Flags containing additional information when sending the list of video sources
	/// </summary>
	[Flags]
	public enum MsexVideoSourcesFlags : ushort
	{
		/// <summary>
		///     No flags
		/// </summary>
		None = 0x0000,

		/// <summary>
		///     Video source is without effects
		/// </summary>
		WithoutEffects = 0x0001
	}
}