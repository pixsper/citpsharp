using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class ClientInformationPacket : MsexPacket
	{
        public ClientInformationPacket()
            : base(MsexMessageType.ClientInformationMessage)
        {
			SupportedMsexVersions = ImmutableHashSet<MsexVersion>.Empty;
        }

	    public ClientInformationPacket(MsexVersion version, IEnumerable<MsexVersion> supportedMsexVersions, ushort requestResponseIndex = 0)
			: base(MsexMessageType.ClientInformationMessage, version, requestResponseIndex)
	    {
	        SupportedMsexVersions = supportedMsexVersions.ToImmutableHashSet();

	    }

	    public ImmutableHashSet<MsexVersion> SupportedMsexVersions { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(SupportedMsexVersions, TypeCode.Byte, 
				v => writer.Write(v, true));
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			SupportedMsexVersions = reader.ReadCollection(TypeCode.Byte, () => reader.ReadMsexVersion(true))
				.ToImmutableHashSet();
		}
	}
}