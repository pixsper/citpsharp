using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class EffectElementInformationPacket : MsexPacket
	{
        public EffectElementInformationPacket()
            : base(MsexMessageType.EffectElementInformationMessage)
        {
			Effects = ImmutableSortedSet<EffectInformation>.Empty;
        }

	    public EffectElementInformationPacket(MsexVersion version, MsexLibraryId libraryId, IEnumerable<EffectInformation> effects,
	        ushort requestResponseIndex = 0)
	        : base(MsexMessageType.EffectElementInformationMessage, version, requestResponseIndex)
	    {
	        LibraryId = libraryId;
	        Effects = effects.ToImmutableSortedSet();
	    }

		public MsexLibraryId LibraryId { get; private set; }

		public ImmutableSortedSet<EffectInformation> Effects { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(LibraryId, Version);
			writer.Write(Effects, GetCollectionLengthType(), e => e.Serialize(writer, Version));
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			LibraryId = reader.ReadLibraryId(Version);
			Effects = reader.ReadCollection(GetCollectionLengthType(),
				() => EffectInformation.Deserialize(reader, Version))
				.ToImmutableSortedSet();
		}
	}
}