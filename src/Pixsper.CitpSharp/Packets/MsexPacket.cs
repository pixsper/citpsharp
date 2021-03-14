using System;
using System.Text;

namespace Pixsper.CitpSharp.Packets
{
	internal abstract class MsexPacket : CitpPacket
	{
		public static readonly int CitpMessageTypePosition = 22;

	    protected MsexPacket(MsexMessageType messageType)
			: base(CitpLayerType.MediaServerExtensionsLayer)
	    {
	        MessageType = messageType;
			Version = MsexVersion.UnsupportedVersion;
		}

		protected MsexPacket(MsexMessageType messageType, MsexVersion version, ushort requestResponseIndex = 0)
			: base(CitpLayerType.MediaServerExtensionsLayer, requestResponseIndex)
		{
			MessageType = messageType;
		    Version = version;
		}

		public MsexMessageType MessageType { get; }

		public MsexVersion Version { get; set; }

		public static MsexMessageType? GetMessageType(byte[] data)
		{
			string typeString = Encoding.UTF8.GetString(data, CitpMessageTypePosition, 4);
			return CitpEnumHelper.GetEnumFromIdString<MsexMessageType>(typeString);
		}

	    protected TypeCode GetCollectionLengthType()
	    {
	        switch (Version)
	        {
				case MsexVersion.Version1_0:
	            case MsexVersion.Version1_1:
	                return TypeCode.Byte;
	            case MsexVersion.Version1_2:
	                return TypeCode.UInt16;
	            default:
	                throw new ArgumentOutOfRangeException();
	        }
	    }

	    protected override void SerializeToStream(CitpBinaryWriter writer)
	    {
	        base.SerializeToStream(writer);

	        writer.Write(Version, false);
	        writer.Write(MessageType.GetCustomAttribute<CitpId>().Id);
	    }

	    protected override void DeserializeFromStream(CitpBinaryReader reader)
	    {
	        base.DeserializeFromStream(reader);

	        Version = reader.ReadMsexVersion(false);

	        if (Version == MsexVersion.UnsupportedVersion)
	            throw new InvalidOperationException("Incorrect or invalid MSEX version");

	        if (MessageType != CitpEnumHelper.GetEnumFromIdString<MsexMessageType>(reader.ReadIdString()))
	            throw new InvalidOperationException("Incorrect or invalid message type");
	    }
	}
}