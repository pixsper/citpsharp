using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xabe.FFmpeg;

namespace Pixsper.CitpSharp.FileSystemMediaServer;

internal class FileSystemMediaServerDevice : ICitpMediaServerDevice, IDisposable
{
	private static readonly ImmutableHashSet<string> ImageFileExtensions = new[]
	{
		".png",
		".jpeg",
		".jpg",
		".bmp"
	}.ToImmutableHashSet();

	private static readonly ImmutableHashSet<string> MovieFileExtensions = new[]
	{
		".mov",
		".mp4",
		".m4v"
	}.ToImmutableHashSet();

	private readonly FileSystemWatcher _watcher;

	private ImmutableDictionary<int, ImmutableDictionary<int, string>> _library = ImmutableDictionary<int, ImmutableDictionary<int, string>>.Empty;
	private ImmutableDictionary<MsexLibraryId, ElementLibrary> _citpLibrary = ImmutableDictionary<MsexLibraryId, ElementLibrary>.Empty;
	private ImmutableDictionary<int, ImmutableDictionary<int, string?>> _thumbnails = ImmutableDictionary<int, ImmutableDictionary<int, string?>>.Empty;

	private readonly string ThumbnailsPath = Path.Combine(Path.GetTempPath(), "CITPSharpThumbs");

	public FileSystemMediaServerDevice(Guid uuid, string peerName, string state, string productName,
		int productVersionMajor, int productVersionMinor, int productVersionBugfix, string libraryRootPath)
	{
		Uuid = uuid;
		PeerName = peerName;
		State = state;

		ProductName = productName;
		ProductVersionMajor = productVersionMajor;
		ProductVersionMinor = productVersionMinor;
		ProductVersionBugfix = productVersionBugfix;

		LibraryRootPath = libraryRootPath;


		if (Directory.Exists(ThumbnailsPath))
		{
			var di = new DirectoryInfo(ThumbnailsPath);
			foreach (var file in di.EnumerateFiles())
				file.Delete();
		}
		else
		{
			Directory.CreateDirectory(ThumbnailsPath);
		}


		_watcher = new FileSystemWatcher
		{
			Path = libraryRootPath,
			NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.LastWrite
			               | NotifyFilters.CreationTime | NotifyFilters.DirectoryName
		};

		_watcher.Changed += (s, e) => buildLibrary();
		_watcher.Deleted += (s, e) => buildLibrary();
		_watcher.Created += (s, e) => buildLibrary();
		_watcher.Renamed += (s, e) => buildLibrary();

		buildLibrary();
		_watcher.EnableRaisingEvents = true;
	}

	public string LibraryRootPath { get; }

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

	public IImmutableDictionary<MsexLibraryId, ElementLibrary> ElementLibraries => _citpLibrary;

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

	public IImmutableDictionary<int, VideoSourceInformation> VideoSourceInformation => ImmutableDictionary<int, VideoSourceInformation>.Empty;


	public IImmutableList<ElementLibraryUpdatedInformation> GetLibraryUpdateInformation()
	{
		return ImmutableList<ElementLibraryUpdatedInformation>.Empty;
	}

	public CitpImage? GetElementLibraryThumbnail(CitpImageRequest request, ElementLibraryInformation elementLibrary)
	{
		return null;
	}

	public CitpImage? GetElementThumbnail(CitpImageRequest request, ElementLibraryInformation elementLibrary,
		ElementInformation element)
	{
		if (!_thumbnails.TryGetValue(elementLibrary.Id.LibraryNumber, out var localLibrary))
			return null;

		if (!localLibrary.TryGetValue(element.ElementNumber, out var thumbnailPath) || thumbnailPath is null)
			return null;

		using (var image = Image.Load(thumbnailPath))
		using (var ms = new MemoryStream())
		{
			image.Mutate(c => c.Resize(request.FrameWidth, request.FrameHeight));
			switch (request.Format)
			{
				case MsexImageFormat.Rgb8:

					var rgbImage = image.CloneAs<Rgb24>();
					rgbImage.ProcessPixelRows(accessor =>
					{
						for (int i = 0; i < accessor.Height; ++i)
						{
							var pixelRow = accessor.GetRowSpan(i);

							if (request.IsBgrOrder)
							{
								foreach (ref var p in pixelRow)
								{
									ms.WriteByte(p.B);
									ms.WriteByte(p.G);
									ms.WriteByte(p.R);
								}
							}
							else
							{
								foreach (ref var p in pixelRow)
								{
									ms.WriteByte(p.R);
									ms.WriteByte(p.G);
									ms.WriteByte(p.B);
								}
							}
						}
					});
					break;

				case MsexImageFormat.Png:
					image.SaveAsPng(ms);
					break;

				case MsexImageFormat.Jpeg:
					image.SaveAsJpeg(ms);
					break;

				default:
					return null;
			}

			var citpImage = new CitpImage(request, ms.ToArray(), image.Width, image.Height);

			return citpImage;
		}
	}

	private void buildLibrary()
	{
		var pathRegex = new Regex(@"(\d{3}).+");

		var directories = Directory.GetDirectories(LibraryRootPath, "", SearchOption.TopDirectoryOnly)
			.Select(p => new { m = pathRegex.Matches(p), p })
			.Where(a => a.m.Any())
			.Select(a => Tuple.Create(int.Parse(a.m[0].Groups[1].Value), a.p))
			.Where(t => t.Item1 >= 0 && t.Item1 <= 255);

		var updatedLibrary = new Dictionary<int, ImmutableDictionary<int, string>>();

		foreach (var dir in directories)
		{
			var files = Directory.GetFiles(dir.Item2, "", SearchOption.TopDirectoryOnly)
				.Where(p => pathRegex.IsMatch(p));

			var libraryFiles = new Dictionary<int, string>();

			foreach (var f in files)
			{
				if (!pathRegex.IsMatch(f))
					continue;

				int index = int.Parse(Path.GetFileNameWithoutExtension(f).Substring(0, 3));

				if (index < 0 || index > 255 || libraryFiles.ContainsKey(index))
					continue;

				libraryFiles.Add(index, f);
			}

			updatedLibrary.Add(dir.Item1, libraryFiles.ToImmutableDictionary());
		}

		_library = updatedLibrary.ToImmutableDictionary();

		_citpLibrary = _library.ToImmutableDictionary(p => MsexLibraryId.FromMsexV1LibraryNumber(p.Key),
			p => new ElementLibrary(MsexLibraryType.Media,
				new ElementLibraryInformation(MsexLibraryId.FromMsexV1LibraryNumber(p.Key),
					(byte)p.Key, (byte)p.Key, p.Key.ToString(), 0, (ushort)p.Value.Count, 0),
				p.Value.Select(createMediaInformation)));

		_thumbnails = _library.ToImmutableDictionary(b => b.Key,
			b => b.Value.ToImmutableDictionary(c => c.Key, c => cacheThumbnail(b.Key, c.Key, c.Value)));
	}

	private MediaInformation createMediaInformation(KeyValuePair<int, string> e)
	{
		var versionRegex = new Regex(@"v(\d+)\.");

		(int key, var value) = e;
		var matches = versionRegex.Matches(value);

		uint serialNumber = 0;

		if (matches.Any())
			serialNumber = uint.Parse(matches[0].Groups[1].Value);

		ushort width = 0;
		ushort height = 0;
		uint length = 0;
		byte fps = 0;


		var infoTask = FFmpeg.GetMediaInfo(value);
		infoTask.Wait();

		var mediaInfo = infoTask.Result;

		if (mediaInfo != null && mediaInfo.VideoStreams.Any())
		{
			width = (ushort)mediaInfo.VideoStreams.First().Width;
			height = (ushort)mediaInfo.VideoStreams.First().Height;
			fps = (byte)mediaInfo.VideoStreams.First().Framerate;
			length = (uint)(mediaInfo.VideoStreams.First().Duration.Seconds * mediaInfo.VideoStreams.First().Framerate);

		}

		return new MediaInformation((byte)key, (byte)key, (byte)key,
			Path.GetFileNameWithoutExtension(value), File.GetLastWriteTime(value),
			width, height, length, fps, serialNumber);
	}

	private string? cacheThumbnail(int libraryIndex, int mediaIndex, string mediaPath)
	{
		var extension = Path.GetExtension(mediaPath);

		Image<Rgba32> image;

		if (MovieFileExtensions.Contains(extension))
		{
			string outputPath = Path.Combine(ThumbnailsPath, $"temp{libraryIndex}-{mediaIndex}" + ".bmp");

			var command = $"-i \"{mediaPath}\" -vf \"thumbnail\" -frames:v 1 \"{outputPath}\"";
			command.RunFfmpeg();

			image = Image.Load(outputPath).CloneAs<Rgba32>();

			File.Delete(outputPath);
		}
		else if (ImageFileExtensions.Contains(extension))
		{
			image = Image.Load(mediaPath).CloneAs<Rgba32>();
		}
		else
		{
			return null;
		}

		string thumbnailPath = Path.Combine(ThumbnailsPath, $"citp{libraryIndex}-{mediaIndex}.png");

		image.Save(thumbnailPath, new PngEncoder());

		return thumbnailPath;
	}

	public void Dispose()
	{
		_watcher.Dispose();
		Directory.Delete(ThumbnailsPath, true);
	}
}