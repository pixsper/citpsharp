namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class ElementLibraryThumbnailPacket : MsexPacket
	{
        public ElementLibraryThumbnailPacket()
            : base(MsexMessageType.ElementLibraryThumbnailMessage)
        {
            ThumbnailBuffer = new byte[0];
        }

	    public ElementLibraryThumbnailPacket(MsexVersion version, MsexLibraryType libraryType, MsexLibraryId libraryId,
	        MsexImageFormat thumbnailFormat, ushort thumbnailWidth, ushort thumbnailHeight, byte[] thumbnailBuffer,
			ushort requestResponseIndex = 0)
			: base(MsexMessageType.ElementLibraryThumbnailMessage, version, requestResponseIndex)
	    {
	        LibraryType = libraryType;
	        LibraryId = libraryId;
	        ThumbnailFormat = thumbnailFormat;
	        ThumbnailWidth = thumbnailWidth;
	        ThumbnailHeight = thumbnailHeight;
	        ThumbnailBuffer = thumbnailBuffer;
	    }

		public MsexLibraryType LibraryType { get; private set; }
		public MsexLibraryId LibraryId { get; private set; }

		public MsexImageFormat ThumbnailFormat { get; private set; }

		public ushort ThumbnailWidth { get; private set; }
		public ushort ThumbnailHeight { get; private set; }
		public byte[] ThumbnailBuffer { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write((byte)LibraryType);
			writer.Write(LibraryId, Version);
			writer.Write(ThumbnailFormat.GetCustomAttribute<CitpId>().Id);
			writer.Write(ThumbnailWidth);
			writer.Write(ThumbnailHeight);
			writer.Write((ushort)ThumbnailBuffer.Length);
			writer.Write(ThumbnailBuffer);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			LibraryType = (MsexLibraryType)reader.ReadByte();
		    LibraryId = reader.ReadLibraryId(Version);

			ThumbnailFormat = CitpEnumHelper.GetEnumFromIdString<MsexImageFormat>(reader.ReadIdString());

			ThumbnailWidth = reader.ReadUInt16();
			ThumbnailHeight = reader.ReadUInt16();

			int thumbnailBufferLength = reader.ReadUInt16();
			ThumbnailBuffer = reader.ReadBytes(thumbnailBufferLength);
		}
	}
}