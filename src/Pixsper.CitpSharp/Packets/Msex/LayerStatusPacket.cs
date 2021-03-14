using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
    internal class LayerStatusPacket : MsexPacket
    {
        public LayerStatusPacket()
            : base(MsexMessageType.LayerStatusMessage)
        {
            LayerStatuses = ImmutableList<LayerStatus>.Empty;
        }

        public LayerStatusPacket(MsexVersion version, IEnumerable<LayerStatus> layerStatuses,
            ushort requestResponseIndex = 0)
            : base(MsexMessageType.LayerStatusMessage, version, requestResponseIndex)
        {
            LayerStatuses = layerStatuses.ToImmutableList();
        }

        public LayerStatusPacket SetVersion(MsexVersion version)
        {
            return new LayerStatusPacket(version, LayerStatuses, RequestResponseIndex);
        }

        public ImmutableList<LayerStatus> LayerStatuses { get; private set; }

        protected override void SerializeToStream(CitpBinaryWriter writer)
        {
            base.SerializeToStream(writer);

            writer.Write(LayerStatuses, TypeCode.Byte, l => l.Serialize(writer, Version));
        }

        protected override void DeserializeFromStream(CitpBinaryReader reader)
        {
            base.DeserializeFromStream(reader);

            LayerStatuses = reader.ReadCollection(TypeCode.Byte, () => LayerStatus.Deserialize(reader, Version))
                .ToImmutableList();
        }



        internal class LayerStatus : IEquatable<LayerStatus>
        {
            public static LayerStatus Deserialize(CitpBinaryReader reader, MsexVersion version)
            {
                byte layerNumber = reader.ReadByte();
                byte physicalOutput = reader.ReadByte();

                var mediaLibraryType = MsexLibraryType.None;
                if (version != MsexVersion.Version1_0)
                    mediaLibraryType = (MsexLibraryType)reader.ReadByte();

                var mediaLibrary = reader.ReadLibraryId(version);
                byte mediaNumber = reader.ReadByte();
                string mediaName = reader.ReadString();
                uint mediaPosition = reader.ReadUInt32();
                uint mediaLength = reader.ReadUInt32();
                byte mediaFps = reader.ReadByte();
                var layerStatusFlags = (MsexLayerStatusFlags)reader.ReadUInt32();

                return new LayerStatus(layerNumber, physicalOutput, mediaLibraryType, mediaLibrary, mediaNumber,
                    mediaName, mediaPosition, mediaLength, mediaFps, layerStatusFlags);
            }

            public LayerStatus(byte layerNumber, byte physicalOutput, MsexLibraryType mediaLibraryType,
                MsexLibraryId mediaLibraryId, byte mediaNumber, string mediaName, uint mediaPosition, uint mediaLength,
                byte mediaFps, MsexLayerStatusFlags layerStatusFlags)
            {
                LayerNumber = layerNumber;
                PhysicalOutput = physicalOutput;
                MediaLibraryType = mediaLibraryType;
                MediaLibraryId = mediaLibraryId;
                MediaNumber = mediaNumber;
                MediaName = mediaName;
                MediaPosition = mediaPosition;
                MediaLength = mediaLength;
                MediaFps = mediaFps;
                LayerStatusFlags = layerStatusFlags;
            }

            public byte LayerNumber { get; }
            public byte PhysicalOutput { get; }


            public MsexLibraryType MediaLibraryType { get; }
            public MsexLibraryId MediaLibraryId { get; }

            public byte MediaNumber { get; }
            public string MediaName { get; }
            public uint MediaPosition { get; }
            public uint MediaLength { get; }
            public byte MediaFps { get; }

            public MsexLayerStatusFlags LayerStatusFlags { get; }


            public void Serialize(CitpBinaryWriter writer, MsexVersion version)
            {
                writer.Write(LayerNumber);
                writer.Write(PhysicalOutput);

                if (version != MsexVersion.Version1_0)
                    writer.Write((byte)MediaLibraryType);

                writer.Write(MediaLibraryId, version);
                writer.Write(MediaNumber);
                writer.Write(MediaName);
                writer.Write(MediaPosition);
                writer.Write(MediaLength);
                writer.Write(MediaFps);
                writer.Write((uint)LayerStatusFlags);
            }

            public bool Equals(LayerStatus? other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return LayerNumber == other.LayerNumber && PhysicalOutput == other.PhysicalOutput
                                                        && MediaLibraryType == other.MediaLibraryType
                                                        && MediaLibraryId.Equals(other.MediaLibraryId)
                                                        && MediaNumber == other.MediaNumber
                                                        && MediaName == other.MediaName
                                                        && MediaPosition == other.MediaPosition
                                                        && MediaLength == other.MediaLength
                                                        && MediaFps == other.MediaFps
                                                        && LayerStatusFlags == other.LayerStatusFlags;
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((LayerStatus)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = LayerNumber.GetHashCode();
                    hashCode = (hashCode * 397) ^ PhysicalOutput.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)MediaLibraryType;
                    hashCode = (hashCode * 397) ^ MediaLibraryId.GetHashCode();
                    hashCode = (hashCode * 397) ^ MediaNumber.GetHashCode();
                    hashCode = (hashCode * 397) ^ MediaName.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)MediaPosition;
                    hashCode = (hashCode * 397) ^ (int)MediaLength;
                    hashCode = (hashCode * 397) ^ MediaFps.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)LayerStatusFlags;
                    return hashCode;
                }
            }
        }
    }
}