using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class ElementLibraryUpdatedPacket : MsexPacket
	{
        public ElementLibraryUpdatedPacket()
            : base(MsexMessageType.ElementLibraryUpdatedMessage)
        {
			AffectedElements = ImmutableSortedSet<byte>.Empty;
			AffectedLibraries = ImmutableSortedSet<byte>.Empty;
        }

	    public ElementLibraryUpdatedPacket(MsexVersion version, MsexLibraryType libraryType, MsexLibraryId libraryId,
	        MsexElementLibraryUpdatedFlags updateFlags, IEnumerable<byte> affectedElements,
	        IEnumerable<byte> affectedLibraries, ushort requestResponseIndex = 0)
	        : base(MsexMessageType.ElementLibraryUpdatedMessage, version, requestResponseIndex)
	    {
	        LibraryType = libraryType;
	        LibraryId = libraryId;
	        UpdateFlags = updateFlags;
	        AffectedElements = affectedElements.ToImmutableSortedSet();
	        AffectedLibraries = affectedLibraries.ToImmutableSortedSet();
	    }

		public MsexLibraryType LibraryType { get; private set; }
		public MsexLibraryId LibraryId { get; private set; }

		public MsexElementLibraryUpdatedFlags UpdateFlags { get; private set; }

		public ImmutableSortedSet<byte> AffectedElements { get; private set; }
		public ImmutableSortedSet<byte> AffectedLibraries { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write((byte)LibraryType);
			writer.Write(LibraryId, Version);
			writer.Write((byte)UpdateFlags);

		    if (Version == MsexVersion.Version1_2)
		    {
				var affectedElements = new BitArray(256);
				foreach (byte a in AffectedElements)
					affectedElements[a] = true;
				var affectedElementsBytes = new byte[32];
				affectedElements.CopyTo(affectedElementsBytes, 0);
				writer.Write(affectedElementsBytes);

				var affectedLibraries = new BitArray(256);
				foreach (byte a in AffectedLibraries)
					affectedLibraries[a] = true;
				var affectedLibrariesBytes = new byte[32];
				affectedLibraries.CopyTo(affectedLibrariesBytes, 0);
				writer.Write(affectedLibrariesBytes);
			}
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			LibraryType = (MsexLibraryType)reader.ReadByte();
			LibraryId = reader.ReadLibraryId(Version);
			UpdateFlags = (MsexElementLibraryUpdatedFlags)reader.ReadByte();

		    if (Version == MsexVersion.Version1_2)
		    {
				var affectedElementsList = new List<byte>();
				var affectedElementsArray = new BitArray(reader.ReadBytes(32));
				for (byte i = 0; i <= 255; ++i)
				{
					if (affectedElementsArray[i])
						affectedElementsList.Add(i);
				}
				AffectedElements = affectedElementsList.ToImmutableSortedSet();

				var affectedLibrariesList = new List<byte>();
				var affectedLibrariesArray = new BitArray(reader.ReadBytes(32));
				for (byte i = 0; i <= 255; ++i)
				{
					if (affectedLibrariesArray[i])
						affectedLibrariesList.Add(i);
				}
				AffectedLibraries = affectedLibrariesList.ToImmutableSortedSet();
			}
		}
	}
}