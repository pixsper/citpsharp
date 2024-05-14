using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class MediaElementInformationPacket : MsexPacket
	{
        public MediaElementInformationPacket()
            : base(MsexMessageType.MediaElementInformationMessage)
        {
			Media = ImmutableSortedSet<MediaInformation>.Empty;
        }

	    public MediaElementInformationPacket(MsexVersion version, MsexLibraryId libraryId, IEnumerable<MediaInformation> media,
	        ushort requestResponseIndex = 0)
	        : base(MsexMessageType.MediaElementInformationMessage, version, requestResponseIndex)
	    {
	        LibraryId = libraryId;
	        Media = media.ToImmutableSortedSet();
	    }

		public MsexLibraryId LibraryId { get; private set; }

		public ImmutableSortedSet<MediaInformation> Media { get; set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(LibraryId, Version);
			writer.Write(Media, GetCollectionLengthType(), m => m.Serialize(writer, Version));
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			LibraryId = reader.ReadLibraryId(Version);
			Media = reader.ReadCollection(GetCollectionLengthType(), () => MediaInformation.Deserialize(reader, Version))
					.ToImmutableSortedSet();
		}
	}
}