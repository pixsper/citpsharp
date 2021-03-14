namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class RequestStreamPacket : MsexPacket
	{
		public RequestStreamPacket()
			: base(MsexMessageType.RequestStreamMessage) { }

		public RequestStreamPacket(MsexVersion version, ushort sourceId, MsexImageFormat frameFormat, ushort frameWidth,
			ushort frameHeight, byte fps, byte timeout, ushort requestResponseIndex = 0)
			: base(MsexMessageType.RequestStreamMessage, version, requestResponseIndex)
		{
			SourceId = sourceId;
			FrameFormat = frameFormat;
			FrameWidth = frameWidth;
			FrameHeight = frameHeight;
			Fps = fps;
			Timeout = timeout;
		}

		public ushort SourceId { get; private set; }
		public MsexImageFormat FrameFormat { get; private set; }

		public ushort FrameWidth { get; private set; }
		public ushort FrameHeight { get; private set; }
		public byte Fps { get; private set; }
		public byte Timeout { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(SourceId);
			writer.Write(FrameFormat.GetCustomAttribute<CitpId>().Id);
			writer.Write(FrameWidth);
			writer.Write(FrameHeight);
			writer.Write(Fps);
			writer.Write(Timeout);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			SourceId = reader.ReadUInt16();
			FrameFormat = CitpEnumHelper.GetEnumFromIdString<MsexImageFormat>(reader.ReadIdString());
			FrameWidth = reader.ReadUInt16();
			FrameHeight = reader.ReadUInt16();
			Fps = reader.ReadByte();
			Timeout = reader.ReadByte();
		}
	}
}