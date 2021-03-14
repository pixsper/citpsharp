namespace Pixsper.CitpSharp.Packets.Pinf
{
	internal class PeerNamePacket : PinfPacket
	{
        public PeerNamePacket()
            : base(PinfMessageType.PeerNameMessage)
        {
            Name = string.Empty;
        }

	    public PeerNamePacket(string name)
	        : base(PinfMessageType.PeerNameMessage)
	    {
	        Name = name;
	    }

		public string Name { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			writer.Write(Name, true);
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			Name = reader.ReadString(true);
		}
	}
}