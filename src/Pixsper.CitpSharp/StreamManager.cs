using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Pixsper.CitpSharp.Packets.Msex;

namespace Pixsper.CitpSharp
{
	internal class StreamSource
	{
		private readonly ILogger _logger;
		private readonly ICitpServerDevice _device;

		private uint _frameIndex;

		private ImmutableDictionary<StreamRequest, DateTime> _requests = ImmutableDictionary<StreamRequest, DateTime>.Empty;
		private ImmutableDictionary<CitpImageRequest, StreamInfo> _resolvedRequests = ImmutableDictionary<CitpImageRequest, StreamInfo>.Empty;

		public StreamSource(ILogger logger, ICitpServerDevice device, ushort sourceId)
		{
			_logger = logger;
			_device = device;
			SourceId = sourceId;
		}


		public event EventHandler<CitpImageRequest>? RequestAdded;
		public event EventHandler<CitpImageRequest>? RequestRemoved;


		public ushort SourceId { get; }

		public IEnumerable<CitpImageRequest> ResolvedRequests => _resolvedRequests.Keys;

		public void AddRequest(PeerInfo peer, RequestStreamPacket packet)
		{
			Debug.Assert(packet.SourceId == SourceId, "Patch source ID does not match this source");

			var request = new StreamRequest(peer, packet.Version, packet.FrameFormat, packet.FrameWidth, packet.FrameHeight, packet.Fps);
			_requests = _requests.SetItem(request, DateTime.Now + TimeSpan.FromSeconds(packet.Timeout));

			computeResolvedRequests();
		}

		public void RemoveExpiredRequests(DateTime timeNow)
		{
			bool isRequestsChanged = false;

			foreach (var pair in _requests)
			{
				if (pair.Value >= timeNow)
					continue;

				_logger.LogInformation("Stream frame request from {Peer} for source {SourceId} timed out", pair.Key.Peer, SourceId);

				_requests = _requests.Remove(pair.Key);
				isRequestsChanged = true;
			}

			if (isRequestsChanged)
				computeResolvedRequests();
		}

		public IEnumerable<StreamFramePacket> GetPackets(DateTime timeNow)
		{
			foreach (var pair in _resolvedRequests)
			{
				if (pair.Value.LastOutput + TimeSpan.FromMilliseconds(1000f / pair.Value.Fps) > timeNow)
					continue;

				var image = _device.GetVideoSourceFrame(SourceId, pair.Key);
				if (image == null)
				{
					//_logger.LogError($"Failed to get image for stream request on source {SourceId}");
					continue;
				}

				if (pair.Key.Format == MsexImageFormat.FragmentedJpeg || pair.Key.Format == MsexImageFormat.FragmentedPng)
				{
					var fragments = image.ImageBuffer.Split(CitpImage.MaximumFragmentedImageBufferLength);

					if (fragments.Length > ushort.MaxValue)
					{
						_logger.LogWarning("Cannot send streaming frame for source {SourceId}, too many image fragments", SourceId);
						continue;
					}

					for (int i = 0; i < fragments.Length; ++i)
					{
					    var fragmentInfo = new StreamFramePacket.FragmentPreamble(_frameIndex, (ushort)fragments.Length, (ushort)i,
					        (uint)(CitpImage.MaximumFragmentedImageBufferLength * i));

					    yield return new StreamFramePacket(pair.Value.Version, _device.Uuid, SourceId, pair.Key.Format,
					        (ushort)image.ActualWidth, (ushort)image.ActualHeight, fragments[i], fragmentInfo);
					}
				}
				else
				{
					if (image.ImageBuffer.Length > CitpImage.MaximumImageBufferLength)
					{
						_logger.LogError("Provided image buffer for source {SourceId} at resolution {FrameWidth} x {FrameHeight}, "
						                 + "format {Format}{IsBgrOrder} is too large to be transported in a single UDP packet", SourceId,
                            pair.Key.FrameWidth, pair.Key.FrameHeight, pair.Key.Format, (pair.Key.IsBgrOrder ? " (BGR Mode)" : ""));

						continue;
					}

					yield return new StreamFramePacket(pair.Value.Version, _device.Uuid, SourceId, pair.Key.Format,
						   (ushort)image.ActualWidth, (ushort)image.ActualHeight, image.ImageBuffer);
				}

				pair.Value.LastOutput = timeNow;
			}

			unchecked
			{
				++_frameIndex;
			}
		}

		private void computeResolvedRequests()
		{
			var updatedRequests = new Dictionary<CitpImageRequest, StreamInfo>();

			foreach (var format in _device.SupportedStreamFormats)
			{
				bool isRequireBgrCompatibilityPacket = false;
				ushort width = 0;
				ushort height = 0;
				byte fps = 0;
				var version = MsexVersion.Version1_2;

				foreach (var pair in _requests.Where(p => p.Key.Format == format))
				{
					if (format == MsexImageFormat.Rgb8 && pair.Key.Version == MsexVersion.Version1_0)
					{
						isRequireBgrCompatibilityPacket = true;
						continue;
					}

					width = Math.Max(width, pair.Key.Width);
					height = Math.Max(height, pair.Key.Height);
					fps = Math.Max(fps, pair.Key.Fps);
					version = (MsexVersion)Math.Min((ushort)version, (ushort)pair.Key.Version);
				}

				if (width == 0 || height == 0 || fps == 0 || version == MsexVersion.UnsupportedVersion)
					continue;

				var request = new CitpImageRequest(width, height, format, true);
				updatedRequests.Add(request, new StreamInfo(fps, version, DateTime.MinValue));

				if (isRequireBgrCompatibilityPacket)
				{
					ushort bgrWidth = 0;
					ushort bgrHeight = 0;
					byte bgrFps = 0;

					foreach (var pair in _requests)
					{
						if (format == MsexImageFormat.Rgb8 && pair.Key.Version == MsexVersion.Version1_0)
						{
							bgrWidth = Math.Max(bgrWidth, pair.Key.Width);
							bgrHeight = Math.Max(bgrHeight, pair.Key.Height);
							bgrFps = Math.Max(bgrFps, pair.Key.Fps);
						}
					}

					var bgrRequest = new CitpImageRequest(width, height, format, true, true);
					updatedRequests.Add(bgrRequest, new StreamInfo(fps, MsexVersion.Version1_0, DateTime.MinValue));
				}
			}

			foreach (var pair in _resolvedRequests)
			{
				if (updatedRequests.ContainsKey(pair.Key))
					continue;

				_resolvedRequests = _resolvedRequests.Remove(pair.Key);
				RequestRemoved?.Invoke(this, pair.Key);
			}

			foreach (var pair in updatedRequests)
			{
				if (_resolvedRequests.ContainsKey(pair.Key))
				{
					_resolvedRequests = _resolvedRequests.SetItem(pair.Key, pair.Value);
				}
				else
				{
					_resolvedRequests = _resolvedRequests.Add(pair.Key, pair.Value);
					RequestAdded?.Invoke(this, pair.Key);
				}
			}
		}



		private class StreamInfo
		{
			public StreamInfo(byte fps, MsexVersion version, DateTime lastOutput)
			{
				Fps = fps;
				Version = version;
				LastOutput = lastOutput;
			}

			public byte Fps { get; }
			public MsexVersion Version { get; }
			public DateTime LastOutput { get; set; }
		}
	}

	internal class StreamManager
	{
	    private readonly ILogger _logger;
	    private readonly ICitpServerDevice _device;

		private ImmutableDictionary<ushort, StreamSource> _streams = ImmutableDictionary<ushort, StreamSource>.Empty;

	    public StreamManager(ILogger logger, ICitpServerDevice device)
	    {
		    _logger = logger;
		    _device = device;
	    }


	    public event EventHandler<CitpImageRequest>? RequestAdded;
	    public event EventHandler<CitpImageRequest>? RequestRemoved;


	    public void AddRequest(PeerInfo peer, RequestStreamPacket packet)
	    {
            if (!_streams.TryGetValue(packet.SourceId, out var source))
		    {
                if (!_device.VideoSourceInformation.TryGetValue(packet.SourceId, out var info))
			    {
				    _logger.LogError("Peer '{Peer}' requested stream whuch does not exist on this server", peer);
				    return;
			    }

			    source = new StreamSource(_logger, _device, info.SourceIdentifier);
			    source.RequestAdded += (s, e) => RequestAdded?.Invoke(s, e);
			    source.RequestRemoved += (s, e) => RequestRemoved?.Invoke(s, e);
				_streams = _streams.Add(info.SourceIdentifier, source);
		    }
		    
			source.AddRequest(peer, packet);
	    }

		public IEnumerable<StreamFramePacket> GetPackets(ushort? sourceId = null)
		{
			var packets = new List<StreamFramePacket>();
			var timeNow = DateTime.Now;

			if (sourceId.HasValue)
			{
                if (_streams.TryGetValue(sourceId.Value, out var source))
				{
					source.RemoveExpiredRequests(timeNow);
					packets.AddRange(source.GetPackets(timeNow));
				}
			}
			else
			{
				foreach (var pair in _streams)
				{
					pair.Value.RemoveExpiredRequests(timeNow);
					packets.AddRange(pair.Value.GetPackets(timeNow));
				}
			}

			return packets;
		}

		public IEnumerable<CitpImageRequest> GetStreamFrameRequests(ushort sourceId)
	    {
			var requests = new HashSet<CitpImageRequest>();

            if (_streams.TryGetValue(sourceId, out var source))
			{
				foreach (var request in source.ResolvedRequests)
					requests.Add(request);
			}

			return requests;
		}
	}
}
