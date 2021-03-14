using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class ElementLibraryInformationPacket : MsexPacket
	{
        public ElementLibraryInformationPacket()
            : base(MsexMessageType.ElementLibraryInformationMessage)
        {
			Elements = ImmutableSortedSet<ElementLibraryInformation>.Empty;
        }

	    public ElementLibraryInformationPacket(MsexVersion version, MsexLibraryType libraryType,
	        IEnumerable<ElementLibraryInformation> elements,
	        ushort requestResponseIndex = 0)
	        : base(MsexMessageType.ElementLibraryInformationMessage, version, requestResponseIndex)
	    {
	        LibraryType = libraryType;
	        Elements = elements.ToImmutableSortedSet();
	    }

		public MsexLibraryType LibraryType { get; private set; }

		public ImmutableSortedSet<ElementLibraryInformation> Elements { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write((byte)LibraryType);
			writer.Write(Elements, GetCollectionLengthType(), e => e.Serialize(writer, Version));
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			LibraryType = (MsexLibraryType)reader.ReadByte();
			Elements = reader.ReadCollection(GetCollectionLengthType(), 
				() => ElementLibraryInformation.Deserialize(reader, Version))
				.ToImmutableSortedSet();
		}
	}
}