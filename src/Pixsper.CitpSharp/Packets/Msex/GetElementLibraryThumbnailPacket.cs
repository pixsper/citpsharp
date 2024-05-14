using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class GetElementLibraryThumbnailPacket : MsexPacket
	{
        public GetElementLibraryThumbnailPacket()
            : base(MsexMessageType.GetElementLibraryThumbnailMessage)
        {
			RequestedLibraryIds = ImmutableSortedSet<MsexLibraryId>.Empty;
        }

	    public GetElementLibraryThumbnailPacket(MsexVersion version, MsexImageFormat thumbnailFormat, ushort thumbnailWidth,
	        ushort thumbnailHeight, MsexThumbnailFlags thumbnailFlags, MsexLibraryType libraryType,
	        IEnumerable<MsexLibraryId> requestedLibrariesIds, ushort requestResponseIndex = 0)
	        : base(MsexMessageType.GetElementLibraryThumbnailMessage, version, requestResponseIndex)
	    {
	        ThumbnailFormat = thumbnailFormat;
	        ThumbnailWidth = thumbnailWidth;
	        ThumbnailHeight = thumbnailHeight;
	        ThumbnailFlags = thumbnailFlags;
	        LibraryType = libraryType;
	        ShouldRequestAllThumbnails = false;
	        RequestedLibraryIds = requestedLibrariesIds.ToImmutableSortedSet();
	    }

		public GetElementLibraryThumbnailPacket(MsexVersion version, MsexImageFormat thumbnailFormat, ushort thumbnailWidth,
			ushort thumbnailHeight, MsexThumbnailFlags thumbnailFlags, MsexLibraryType libraryType, ushort requestResponseIndex = 0)
			: base(MsexMessageType.GetElementLibraryThumbnailMessage, version, requestResponseIndex)
		{
			ThumbnailFormat = thumbnailFormat;
			ThumbnailWidth = thumbnailWidth;
			ThumbnailHeight = thumbnailHeight;
			ThumbnailFlags = thumbnailFlags;
			LibraryType = libraryType;
			ShouldRequestAllThumbnails = true;
			RequestedLibraryIds = ImmutableSortedSet<MsexLibraryId>.Empty;
		}

		public MsexImageFormat ThumbnailFormat { get; private set; }

		public ushort ThumbnailWidth { get; private set; }
		public ushort ThumbnailHeight { get; private set; }
		public MsexThumbnailFlags ThumbnailFlags { get; private set; }

		public MsexLibraryType LibraryType { get; private set; }

		public bool ShouldRequestAllThumbnails { get; private set; }

		public ImmutableSortedSet<MsexLibraryId> RequestedLibraryIds { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(ThumbnailFormat.GetCustomAttribute<CitpId>().Id);
			writer.Write(ThumbnailWidth);
			writer.Write(ThumbnailHeight);
			writer.Write((byte)ThumbnailFlags);
			writer.Write((byte)LibraryType);
			if (ShouldRequestAllThumbnails)
				writer.Write((byte)0);
			else
				writer.Write(RequestedLibraryIds, GetCollectionLengthType(),
				   i => writer.Write(i, Version));
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			ThumbnailFormat = CitpEnumHelper.GetEnumFromIdString<MsexImageFormat>(reader.ReadIdString());
			ThumbnailWidth = reader.ReadUInt16();
			ThumbnailHeight = reader.ReadUInt16();
			ThumbnailFlags = (MsexThumbnailFlags)reader.ReadByte();
			LibraryType = (MsexLibraryType)reader.ReadByte();

			RequestedLibraryIds = reader.ReadCollection(GetCollectionLengthType(), 
				() => reader.ReadLibraryId(Version)).ToImmutableSortedSet();

			if (RequestedLibraryIds.Count == 0)
				ShouldRequestAllThumbnails = true;
		}
	}
}