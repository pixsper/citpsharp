using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class GetElementLibraryInformationPacket : MsexPacket
	{
        public GetElementLibraryInformationPacket()
            : base(MsexMessageType.GetElementLibraryInformationMessage)
        {
			RequestedLibraryNumbers = ImmutableSortedSet<byte>.Empty;
        }

		public GetElementLibraryInformationPacket(MsexVersion version, MsexLibraryType libraryType,
			MsexLibraryId? libraryParentId, IEnumerable<byte> requestedLibraryNumbers, ushort requestResponseIndex = 0)
			: base(MsexMessageType.GetElementLibraryInformationMessage, version, requestResponseIndex)
		{
			LibraryType = libraryType;

			if (version != MsexVersion.Version1_0 && libraryParentId == null)
				throw new ArgumentNullException(nameof(libraryParentId), "Cannot be null for Msex V1.1+");

			LibraryParentId = libraryParentId;
			ShouldRequestAllLibraries = false;
			RequestedLibraryNumbers = requestedLibraryNumbers.ToImmutableSortedSet();
		}

		public GetElementLibraryInformationPacket(MsexVersion version, MsexLibraryType libraryType,
			MsexLibraryId? libraryParentId, ushort requestResponseIndex = 0)
			: base(MsexMessageType.GetElementLibraryInformationMessage, version, requestResponseIndex)
		{
			LibraryType = libraryType;

			if (version != MsexVersion.Version1_0 && libraryParentId == null)
				throw new ArgumentNullException(nameof(libraryParentId), "Cannot be null for Msex V1.1+");

			LibraryParentId = libraryParentId;
			ShouldRequestAllLibraries = true;
			RequestedLibraryNumbers = ImmutableSortedSet<byte>.Empty;
		}

		public MsexLibraryType LibraryType { get; private set; }
		public MsexLibraryId? LibraryParentId { get; private set; }
		public bool ShouldRequestAllLibraries { get; private set; }
		public ImmutableSortedSet<byte> RequestedLibraryNumbers { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write((byte)LibraryType);

			if (Version != MsexVersion.Version1_0)
			{
                Debug.Assert(LibraryParentId.HasValue); // This should be enforced in the constructor
                writer.Write(LibraryParentId!.Value, Version);
			}

			if (ShouldRequestAllLibraries)
				writer.Write((ushort)0);
			else
				writer.Write(RequestedLibraryNumbers, GetCollectionLengthType(), writer.Write);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			LibraryType = (MsexLibraryType)reader.ReadByte();

			if (Version != MsexVersion.Version1_0)
				LibraryParentId = reader.ReadLibraryId(Version);

			RequestedLibraryNumbers = reader.ReadCollection(GetCollectionLengthType(), reader.ReadByte).ToImmutableSortedSet();

			if (RequestedLibraryNumbers.Count == 0)
				ShouldRequestAllLibraries = true;
		}
	}
}