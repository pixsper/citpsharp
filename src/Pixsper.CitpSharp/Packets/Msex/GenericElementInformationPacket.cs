using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class GenericElementInformationPacket : MsexPacket
	{
        public GenericElementInformationPacket()
            : base(MsexMessageType.GenericElementInformationMessage)
        {
			Information = ImmutableSortedSet<GenericInformation>.Empty;
        }

	    public GenericElementInformationPacket(MsexVersion version, MsexLibraryType libraryType, MsexLibraryId libraryId,
	        IEnumerable<GenericInformation> information, ushort requestResponseIndex = 0)
	        : base(MsexMessageType.GenericElementInformationMessage, version, requestResponseIndex)
	    {
	        LibraryType = libraryType;
	        LibraryId = libraryId;
	        Information = information.ToImmutableSortedSet();
	    }

		public MsexLibraryType LibraryType { get; private set; }
		public MsexLibraryId LibraryId { get; private set; }

		public ImmutableSortedSet<GenericInformation> Information { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write((byte)LibraryType);
			writer.Write(LibraryId, Version);
			writer.Write(Information, GetCollectionLengthType(), e => e.Serialize(writer, Version));
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			if (Version == MsexVersion.Version1_2)
				LibraryType = (MsexLibraryType)reader.ReadByte();

			LibraryId = reader.ReadLibraryId(Version);
			Information = reader.ReadCollection(GetCollectionLengthType(), 
				() => GenericInformation.Deserialize(reader, Version))
					.ToImmutableSortedSet();
		}
	}
}