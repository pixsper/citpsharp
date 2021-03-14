using System;
using System.Diagnostics;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class StreamFramePacket : MsexPacket
	{
        public StreamFramePacket()
            : base(MsexMessageType.StreamFrameMessage)
        {
            FrameBuffer = new byte[0];
        }

		public StreamFramePacket(MsexVersion version, Guid mediaServerUuid, ushort sourceId, MsexImageFormat frameFormat,
			ushort frameWidth, ushort frameHeight, byte[] frameBuffer, FragmentPreamble? fragmentInfo = null,
			ushort requestResponseIndex = 0)
			: base(MsexMessageType.StreamFrameMessage, version, requestResponseIndex)
		{
			MediaServerUuid = mediaServerUuid;
			SourceId = sourceId;

			if (FrameFormat == MsexImageFormat.FragmentedJpeg || FrameFormat == MsexImageFormat.FragmentedPng)
			{
				if (Version != MsexVersion.Version1_2)
					throw new InvalidOperationException("Fragmented frame formats only supported in Msex V1.2+");

				if (FragmentInfo == null)
					throw new ArgumentNullException(nameof(fragmentInfo), "Cannot be null when using a fragmented image format");
			}

			FrameFormat = frameFormat;
			FrameWidth = frameWidth;
			FrameHeight = frameHeight;
			FrameBuffer = frameBuffer;
			FragmentInfo = fragmentInfo;
		}

		public Guid MediaServerUuid { get; private set; }
		public ushort SourceId { get; private set; }
		public MsexImageFormat FrameFormat { get; private set; }
		public ushort FrameWidth { get; private set; }
		public ushort FrameHeight { get; private set; }
		public byte[] FrameBuffer { get; private set; }

		public FragmentPreamble? FragmentInfo { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			if (Version == MsexVersion.Version1_2)
				writer.Write(MediaServerUuid);

			writer.Write(SourceId);
			writer.Write(FrameFormat.GetCustomAttribute<CitpId>().Id);
			writer.Write(FrameWidth);
			writer.Write(FrameHeight);

			if (Version == MsexVersion.Version1_2 && FrameFormat == MsexImageFormat.FragmentedJpeg || FrameFormat == MsexImageFormat.FragmentedPng)
			{
				writer.Write((ushort)(FrameBuffer.Length + FragmentPreamble.ByteLength));

				Debug.Assert(FragmentInfo != null); // Should be checked in constructor
				FragmentInfo!.Serialize(writer);
			}
			else
			{
				writer.Write((ushort)FrameBuffer.Length);
			}

			writer.Write(FrameBuffer);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			if (Version == MsexVersion.Version1_2)
				MediaServerUuid = reader.ReadGuid();

			SourceId = reader.ReadUInt16();
			FrameFormat = CitpEnumHelper.GetEnumFromIdString<MsexImageFormat>(reader.ReadIdString());
			FrameWidth = reader.ReadUInt16();
			FrameHeight = reader.ReadUInt16();

			ushort frameBufferLength = reader.ReadUInt16();

			if (FrameFormat == MsexImageFormat.FragmentedJpeg || FrameFormat == MsexImageFormat.FragmentedPng)
			{
				FragmentInfo = FragmentPreamble.Deserialize(reader);
				FrameBuffer = reader.ReadBytes(frameBufferLength - FragmentPreamble.ByteLength);
			}
			else
			{
				FrameBuffer = reader.ReadBytes(frameBufferLength);
			}
		}



		public class FragmentPreamble : IEquatable<FragmentPreamble>
		{
			public const int ByteLength = 12;

			public static FragmentPreamble Deserialize(CitpBinaryReader reader)
			{
				uint frameIndex = reader.ReadUInt32();
				ushort fragmentCount = reader.ReadUInt16();
				ushort fragmentIndex = reader.ReadUInt16();
				uint fragmentByteOffset = reader.ReadUInt32();

				return new FragmentPreamble(frameIndex, fragmentCount, fragmentIndex, fragmentByteOffset);
			}

			public FragmentPreamble(uint frameIndex, ushort fragmentCount, ushort fragmentIndex, uint fragmentByteOffset)
			{
				FrameIndex = frameIndex;
				FragmentCount = fragmentCount;
				FragmentIndex = fragmentIndex;
				FragmentByteOffset = fragmentByteOffset;
			}

			public uint FrameIndex { get; }
			public ushort FragmentCount { get; }
			public ushort FragmentIndex { get; }
			public uint FragmentByteOffset { get; }

			public void Serialize(CitpBinaryWriter writer)
			{
				writer.Write(FrameIndex);
				writer.Write(FragmentCount);
				writer.Write(FragmentIndex);
				writer.Write(FragmentByteOffset);
			}

            public bool Equals(FragmentPreamble? other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return FrameIndex == other.FrameIndex && FragmentCount == other.FragmentCount && FragmentIndex == other.FragmentIndex && FragmentByteOffset == other.FragmentByteOffset;
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != this.GetType())
                    return false;
                return Equals((FragmentPreamble)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (int)FrameIndex;
                    hashCode = (hashCode * 397) ^ FragmentCount.GetHashCode();
                    hashCode = (hashCode * 397) ^ FragmentIndex.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)FragmentByteOffset;
                    return hashCode;
                }
            }
        }
	}
}