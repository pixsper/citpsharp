using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class GetElementThumbnailPacket : MsexPacket
	{
        public GetElementThumbnailPacket()
            : base(MsexMessageType.GetElementThumbnailMessage)
        {
			RequestedElementNumbers = ImmutableSortedSet<byte>.Empty;
        }

		public GetElementThumbnailPacket(MsexVersion version, MsexImageFormat thumbnailFormat, ushort thumbnailWidth,
			ushort thumbnailHeight, MsexThumbnailFlags thumbnailFlags, MsexLibraryType libraryType, MsexLibraryId libraryId,
			IEnumerable<byte> requestedElementNumbers, ushort requestResponseIndex = 0)
			: base(MsexMessageType.GetElementThumbnailMessage, version, requestResponseIndex)
		{
			ThumbnailFormat = thumbnailFormat;
			ThumbnailWidth = thumbnailWidth;
			ThumbnailHeight = thumbnailHeight;
			ThumbnailFlags = thumbnailFlags;
			LibraryType = libraryType;
			LibraryId = libraryId;
			ShouldRequestAllThumbnails = false;
			RequestedElementNumbers = requestedElementNumbers.ToImmutableSortedSet();
		}

		public GetElementThumbnailPacket(MsexVersion version, MsexImageFormat thumbnailFormat, ushort thumbnailWidth,
	        ushort thumbnailHeight, MsexThumbnailFlags thumbnailFlags, MsexLibraryType libraryType, MsexLibraryId libraryId,
	        ushort requestResponseIndex = 0)
	        : base(MsexMessageType.GetElementThumbnailMessage, version, requestResponseIndex)
	    {
	        ThumbnailFormat = thumbnailFormat;
	        ThumbnailWidth = thumbnailWidth;
	        ThumbnailHeight = thumbnailHeight;
	        ThumbnailFlags = thumbnailFlags;
	        LibraryType = libraryType;
	        LibraryId = libraryId;
	        ShouldRequestAllThumbnails = true;
			RequestedElementNumbers = ImmutableSortedSet<byte>.Empty;
	    }

		public MsexImageFormat ThumbnailFormat { get; private set; }

		public ushort ThumbnailWidth { get; private set; }
		public ushort ThumbnailHeight { get; private set; }
		public MsexThumbnailFlags ThumbnailFlags { get; private set; }

		public MsexLibraryType LibraryType { get; private set; }
		public MsexLibraryId LibraryId { get; private set; }

		public bool ShouldRequestAllThumbnails { get; private set; }

		public ImmutableSortedSet<byte> RequestedElementNumbers { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(ThumbnailFormat.GetCustomAttribute<CitpId>().Id);
			writer.Write(ThumbnailWidth);
			writer.Write(ThumbnailHeight);
			writer.Write((byte)ThumbnailFlags);
			writer.Write((byte)LibraryType);
			writer.Write(LibraryId, Version);

			if (ShouldRequestAllThumbnails)
				writer.Write((byte)0x00);
			else
				writer.Write(RequestedElementNumbers, GetCollectionLengthType(), writer.Write);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			ThumbnailFormat = CitpEnumHelper.GetEnumFromIdString<MsexImageFormat>(reader.ReadIdString());
			ThumbnailWidth = reader.ReadUInt16();
			ThumbnailHeight = reader.ReadUInt16();
			ThumbnailFlags = (MsexThumbnailFlags)reader.ReadByte();
			LibraryType = (MsexLibraryType)reader.ReadByte();
			LibraryId = reader.ReadLibraryId(Version);

			RequestedElementNumbers = reader.ReadCollection(GetCollectionLengthType(), reader.ReadByte).ToImmutableSortedSet();

			if (RequestedElementNumbers.Count == 0)
				ShouldRequestAllThumbnails = true;
		}
	}
}