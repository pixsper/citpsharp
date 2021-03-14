using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class GetElementInformationPacket : MsexPacket
	{
        public GetElementInformationPacket()
            : base(MsexMessageType.GetElementInformationMessage)
        {
			RequestedElementNumbers = ImmutableSortedSet<byte>.Empty;
        }

	    public GetElementInformationPacket(MsexVersion version, MsexLibraryType libraryType, MsexLibraryId libraryId,
	        IEnumerable<byte> requestedElementNumbers, ushort requestResponseIndex = 0)
	        : base(MsexMessageType.GetElementInformationMessage, version, requestResponseIndex)
	    {
	        LibraryType = libraryType;
	        LibraryId = libraryId;
	        ShouldRequestAllElements = false;
	        RequestedElementNumbers = requestedElementNumbers.ToImmutableSortedSet();
	    }

		public GetElementInformationPacket(MsexVersion version, MsexLibraryType libraryType, MsexLibraryId libraryId, ushort requestResponseIndex = 0)
		   : base(MsexMessageType.GetElementInformationMessage, version, requestResponseIndex)
		{
			LibraryType = libraryType;
			LibraryId = libraryId;
			ShouldRequestAllElements = true;
			RequestedElementNumbers = ImmutableSortedSet<byte>.Empty;
		}

		public MsexLibraryType LibraryType { get; private set; }
		public MsexLibraryId LibraryId { get; private set; }

		public bool ShouldRequestAllElements { get; private set; }
		public ImmutableSortedSet<byte> RequestedElementNumbers { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write((byte)LibraryType);
			writer.Write(LibraryId, Version);

			if (ShouldRequestAllElements)
				writer.Write((ushort)0x00);
			else
				writer.Write(RequestedElementNumbers, GetCollectionLengthType(), writer.Write);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			LibraryType = (MsexLibraryType)reader.ReadByte();
			LibraryId = reader.ReadLibraryId(Version);

			RequestedElementNumbers = reader.ReadCollection(GetCollectionLengthType(), reader.ReadByte).ToImmutableSortedSet();

			if (RequestedElementNumbers.Count == 0)
				ShouldRequestAllElements = true;
		}
	}
}