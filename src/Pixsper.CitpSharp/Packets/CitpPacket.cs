using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Pixsper.CitpSharp.Packets.Msex;
using Pixsper.CitpSharp.Packets.Pinf;

namespace Pixsper.CitpSharp.Packets
{
	internal abstract class CitpPacket
	{
		public static readonly byte[] CitpCookie = Encoding.UTF8.GetBytes("CITP");
		private const byte CitpVersionMajor = 1;
		private const byte CitpVersionMinor = 0;

		private const int HeaderLength = 20;

		public const int PacketLengthIndex = 8;
		public const int ContentTypeIndex = 16;

		public const int MinimumPacketLength = 24;


		protected CitpPacket(CitpLayerType layerType, ushort requestResponseIndex = 0)
		{
			LayerType = layerType;
		    RequestResponseIndex = requestResponseIndex;
			MessagePart = 0;
			MessagePartCount = 1;
		}

		public CitpLayerType LayerType { get; }

		public ushort RequestResponseIndex { get; private set; }
		public ushort MessagePartCount { get; private set; }
		public ushort MessagePart { get; private set; }


		public static CitpPacket FromByteArray(byte[] data)
		{
			CitpPacket packet;

			var layerType = getLayerType(data);

			if (layerType == null)
			{
				var layerTypeArray = new byte[4];
				Buffer.BlockCopy(data, ContentTypeIndex, layerTypeArray, 0, 4);
				throw new InvalidOperationException(
					$"Unrecognised CITP content type: {Encoding.UTF8.GetString(layerTypeArray, 0, layerTypeArray.Length)}");
			}

			switch (layerType)
			{
				case CitpLayerType.PeerInformationLayer:
				{
					var messageType = PinfPacket.GetMessageType(data);

					if (messageType == null)
					{
						var messageTypeArray = new byte[4];
						Buffer.BlockCopy(data, PinfPacket.CitpMessageTypePosition, messageTypeArray, 0, 4);
						throw new InvalidOperationException(
							$"Unrecognised PING message type: {Encoding.UTF8.GetString(messageTypeArray, 0, messageTypeArray.Length)}");
					}

                    packet = messageType switch
                    {
                        PinfMessageType.PeerLocationMessage => new PeerLocationPacket(),
                        PinfMessageType.PeerNameMessage => new PeerNamePacket(),
                        _ => throw new NotImplementedException("Unimplemented PINF message type")
                    };

                    break;
				}

				case CitpLayerType.MediaServerExtensionsLayer:
				{
					var messageType = MsexPacket.GetMessageType(data);

					if (messageType == null)
					{
						var messageTypeArray = new byte[4];
						Buffer.BlockCopy(data, MsexPacket.CitpMessageTypePosition, messageTypeArray, 0, 4);
						throw new InvalidOperationException(
							$"Unrecognised MSEX message type: {Encoding.UTF8.GetString(messageTypeArray, 0, messageTypeArray.Length)}");
					}

                    packet = messageType switch
                    {
                        MsexMessageType.ClientInformationMessage => new ClientInformationPacket(),
                        MsexMessageType.ServerInformationMessage => new ServerInformationPacket(),
                        MsexMessageType.NegativeAcknowledgeMessage => new NegativeAcknowledgePacket(),
                        MsexMessageType.LayerStatusMessage => new LayerStatusPacket(),
                        MsexMessageType.GetElementLibraryInformationMessage => new GetElementLibraryInformationPacket(),
                        MsexMessageType.ElementLibraryInformationMessage => new ElementLibraryInformationPacket(),
                        MsexMessageType.ElementLibraryUpdatedMessage => new ElementLibraryUpdatedPacket(),
                        MsexMessageType.GetElementInformationMessage => new GetElementInformationPacket(),
                        MsexMessageType.MediaElementInformationMessage => new MediaElementInformationPacket(),
                        MsexMessageType.EffectElementInformationMessage => new EffectElementInformationPacket(),
                        MsexMessageType.GenericElementInformationMessage => new GenericElementInformationPacket(),
                        MsexMessageType.GetElementLibraryThumbnailMessage => new GetElementLibraryThumbnailPacket(),
                        MsexMessageType.ElementLibraryThumbnailMessage => new ElementLibraryThumbnailPacket(),
                        MsexMessageType.GetElementThumbnailMessage => new GetElementThumbnailPacket(),
                        MsexMessageType.ElementThumbnailMessage => new ElementThumbnailPacket(),
                        MsexMessageType.GetVideoSourcesMessage => new GetVideoSourcesPacket(),
                        MsexMessageType.RequestStreamMessage => new RequestStreamPacket(),
                        MsexMessageType.StreamFrameMessage => new StreamFramePacket(),
                        _ => throw new NotImplementedException("Unimplemented MSEX message type")
                    };

                    break;
				}

				default:
					throw new NotImplementedException("Unimplemented CITP content type");
			}
			
			using (var reader = new CitpBinaryReader(new MemoryStream(data)))
				packet.DeserializeFromStream(reader);

			return packet;
		}

		private static CitpLayerType? getLayerType(byte[] data)
		{
			string typeString = Encoding.UTF8.GetString(data, ContentTypeIndex, 4);
			return CitpEnumHelper.GetEnumFromIdString<CitpLayerType>(typeString);
		}


		public byte[] ToByteArray()
		{
			return serializePacket();
		}

		public IEnumerable<byte[]> ToByteArray(int maximumPacketSize, int requestResponseIndex = 0)
		{
			var packets = new List<byte[]>();

			var fullData = serializePacket(false);

			int maximumDataSize = maximumPacketSize - HeaderLength;

			int numPackets = (int)Math.Ceiling(fullData.Length / (float)maximumDataSize);

			for (int i = 0; i < numPackets; ++i)
			{
				int packetDataLength;

				if (i == numPackets - 1)
					packetDataLength = fullData.Length % maximumDataSize;
				else
					packetDataLength = maximumDataSize;

				var packet = new byte[packetDataLength + HeaderLength];

				Buffer.BlockCopy(fullData, i * maximumDataSize, packet, HeaderLength, packetDataLength);

				writeInHeader(packet, requestResponseIndex, i, numPackets);

				packets.Add(packet);
			}

			return packets;
		}

		private byte[] serializePacket(bool isAddHeader = true)
		{
			byte[] data;

			using (var writer = new CitpBinaryWriter(new MemoryStream()))
			{
				if (isAddHeader)
					writer.Write(new byte[HeaderLength]);

				SerializeToStream(writer);

				data = ((MemoryStream)writer.BaseStream).ToArray();
			}

			if (isAddHeader)
				writeInHeader(data, RequestResponseIndex, MessagePart, MessagePartCount);

			return data;
		}

		private void writeInHeader(byte[] data, int requestResponseIndex, int messagePart, int messagePartCount)
		{
			Buffer.BlockCopy(CitpCookie, 0, data, 0, 4);

			data[4] = CitpVersionMajor;
			data[5] = CitpVersionMinor;

			unchecked
			{
				data[6] = (byte)requestResponseIndex;
				data[7] = (byte)(requestResponseIndex >> 8);

				data[8] = (byte)data.Length;
				data[9] = (byte)(data.Length >> 8);
				data[10] = (byte)(data.Length >> 16);
				data[11] = (byte)(data.Length >> 24);

				data[12] = (byte)messagePartCount;
				data[13] = (byte)(messagePartCount >> 8);

				data[14] = (byte)messagePart;
				data[15] = (byte)(messagePart >> 8);
			}

			Buffer.BlockCopy(LayerType.GetCustomAttribute<CitpId>().Id, 0, data, 16, 4);
		}



		protected virtual void SerializeToStream(CitpBinaryWriter writer) { }

		protected virtual void DeserializeFromStream(CitpBinaryReader reader)
		{
			reader.ReadBytes(CitpCookie.Length);

		    byte versionMajor = reader.ReadByte();
		    byte versionMinor = reader.ReadByte();

		    if (versionMajor != CitpVersionMajor || versionMinor != CitpVersionMinor)
		        throw new InvalidOperationException($"Unsupported CITP version: v{versionMajor}.{versionMinor}");

		    RequestResponseIndex = reader.ReadUInt16();

		    uint messageSize = reader.ReadUInt32();
		    MessagePartCount = reader.ReadUInt16();
		    MessagePart = reader.ReadUInt16();

		    var layerType = CitpEnumHelper.GetEnumFromIdString<CitpLayerType>(reader.ReadIdString());
			Debug.Assert(layerType == LayerType);
		}
	}
}