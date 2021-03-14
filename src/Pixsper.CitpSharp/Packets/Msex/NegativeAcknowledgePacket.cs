namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class NegativeAcknowledgePacket : MsexPacket
	{
		public NegativeAcknowledgePacket()
			: base(MsexMessageType.NegativeAcknowledgeMessage) { }

	    public NegativeAcknowledgePacket(MsexVersion version, MsexMessageType receivedContentType, ushort requestResponseIndex = 0)
	        : base(MsexMessageType.NegativeAcknowledgeMessage, version, requestResponseIndex)
	    {
	        ReceivedContentType = receivedContentType;
	    }

		public MsexMessageType ReceivedContentType { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(ReceivedContentType.GetCustomAttribute<CitpId>().Id);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			ReceivedContentType = CitpEnumHelper.GetEnumFromIdString<MsexMessageType>(reader.ReadIdString());
		}
	}
}