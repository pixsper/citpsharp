using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.DummyMediaServer;

internal class DummyMediaServerDevice : ICitpMediaServerDevice
{
	public DummyMediaServerDevice(Guid uuid, string peerName, string state, string productName,
		int productVersionMajor, int productVersionMinor, int productVersionBugfix)
	{
			Uuid = uuid;
			PeerName = peerName;
			State = state;

			ProductName = productName;
			ProductVersionMajor = productVersionMajor;
			ProductVersionMinor = productVersionMinor;
			ProductVersionBugfix = productVersionBugfix;
		}

	public Guid Uuid { get; }
	public string PeerName { get; }
	public string State { get; set; }

	public string ProductName { get; }
	public int ProductVersionMajor { get; }
	public int ProductVersionMinor { get; }
	public int ProductVersionBugfix { get; }

	public IImmutableSet<MsexVersion> SupportedMsexVersions =>
		new[]
			{
				MsexVersion.Version1_0,
				MsexVersion.Version1_1,
				MsexVersion.Version1_2
			}.ToImmutableHashSet();


	public IImmutableSet<MsexLibraryType> SupportedLibraryTypes =>
		new[]
			{
				MsexLibraryType.Media
			}.ToImmutableHashSet();

	public IImmutableSet<MsexImageFormat> SupportedThumbnailFormats =>
		new[]
			{
				MsexImageFormat.Rgb8,
				MsexImageFormat.Jpeg,
				MsexImageFormat.Png
			}.ToImmutableHashSet();

	public IImmutableList<ICitpMediaServerLayer> Layers { get; } = ImmutableList<ICitpMediaServerLayer>.Empty;

	public IImmutableDictionary<MsexLibraryId, ElementLibrary> ElementLibraries { get; } =
		ImmutableDictionary<MsexLibraryId, ElementLibrary>.Empty;

	public bool HasLibraryBeenUpdated { get; set; }



	public CitpImage GetVideoSourceFrame(int sourceId, CitpImageRequest request)
	{
			var buffer = new byte[request.FrameWidth * request.FrameHeight * 3];

			for (int i = 0; i < buffer.Length; i += 3)
				buffer[i] = 255;

			return new CitpImage(request, buffer, request.FrameWidth, request.FrameHeight);
		}

	public IImmutableSet<MsexImageFormat> SupportedStreamFormats =>
		new[]
			{
				MsexImageFormat.Rgb8,
				MsexImageFormat.Jpeg,
				MsexImageFormat.Png,
				MsexImageFormat.FragmentedJpeg,
				MsexImageFormat.FragmentedPng
			}.ToImmutableHashSet();

	public IImmutableDictionary<int, VideoSourceInformation> VideoSourceInformation =>
		new Dictionary<int, VideoSourceInformation>
		{
			{1, new VideoSourceInformation(1, "Red", MsexVideoSourcesFlags.None, 1920, 1080)}
		}.ToImmutableDictionary();


	public IImmutableList<ElementLibraryUpdatedInformation> GetLibraryUpdateInformation()
	{
			return ImmutableList<ElementLibraryUpdatedInformation>.Empty;
		}

	public CitpImage GetElementLibraryThumbnail(CitpImageRequest request, ElementLibraryInformation elementLibrary)
	{
			throw new NotImplementedException();
		}

	public CitpImage GetElementThumbnail(CitpImageRequest request, ElementLibraryInformation elementLibrary,
		ElementInformation element)
	{
			throw new NotImplementedException();
		}
}