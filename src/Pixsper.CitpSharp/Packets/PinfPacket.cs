using System;
using System.Text;

namespace Pixsper.CitpSharp.Packets
{
	internal abstract class PinfPacket : CitpPacket
	{
		public static readonly int CitpMessageTypePosition = 20;

		protected PinfPacket(PinfMessageType messageType, ushort requestResponseIndex = 0)
			: base(CitpLayerType.PeerInformationLayer, requestResponseIndex)
		{
			MessageType = messageType;
		}

		public PinfMessageType MessageType { get; }

		public static PinfMessageType? GetMessageType(byte[] data)
		{
			string typeString = Encoding.UTF8.GetString(data, CitpMessageTypePosition, 4);
			return CitpEnumHelper.GetEnumFromIdString<PinfMessageType>(typeString);
		}

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(MessageType.GetCustomAttribute<CitpId>().Id);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			if (MessageType != CitpEnumHelper.GetEnumFromIdString<PinfMessageType>(reader.ReadIdString()))
				throw new InvalidOperationException("Incorrect message type");
		}
	}
}