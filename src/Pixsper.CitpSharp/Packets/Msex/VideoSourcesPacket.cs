using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class VideoSourcesPacket : MsexPacket
	{
        public VideoSourcesPacket()
            : base(MsexMessageType.VideoSourcesMessage)
        {
			Sources = ImmutableSortedSet<VideoSourceInformation>.Empty;
        }

		public VideoSourcesPacket(MsexVersion version, IEnumerable<VideoSourceInformation> sources,
			ushort requestResponseIndex = 0)
			: base(MsexMessageType.VideoSourcesMessage, version, requestResponseIndex)
		{
			Sources = sources.ToImmutableSortedSet();
		}

		public ImmutableSortedSet<VideoSourceInformation> Sources { get; private set; }


		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(Sources, TypeCode.UInt16, s => s.Serialize(writer));
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			Sources = reader.ReadCollection(TypeCode.UInt16, () => VideoSourceInformation.Deserialize(reader))
				.ToImmutableSortedSet();
		}
	}
}