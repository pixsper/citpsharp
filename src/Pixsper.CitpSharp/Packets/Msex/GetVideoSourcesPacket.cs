namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class GetVideoSourcesPacket : MsexPacket
	{
		public GetVideoSourcesPacket()
			: base(MsexMessageType.GetVideoSourcesMessage) { }

	    public GetVideoSourcesPacket(MsexVersion version, ushort requestResponseIndex = 0)
	        : base(MsexMessageType.GetVideoSourcesMessage, version, requestResponseIndex)
	    {
	        
	    }
	}
}