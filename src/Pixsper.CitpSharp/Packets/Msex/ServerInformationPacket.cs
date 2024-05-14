using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp.Packets.Msex
{
	internal class ServerInformationPacket : MsexPacket
	{
        public ServerInformationPacket()
            : base(MsexMessageType.ServerInformationMessage)
        {
            ProductName = string.Empty;
			SupportedMsexVersions = ImmutableHashSet<MsexVersion>.Empty;
			SupportedLibraryTypes = ImmutableHashSet<MsexLibraryType>.Empty;
			ThumbnailFormats = ImmutableHashSet<MsexImageFormat>.Empty;
			StreamFormats = ImmutableHashSet<MsexImageFormat>.Empty;
			LayerDmxSources = ImmutableList<DmxPatchInfo>.Empty;
        }

		public ServerInformationPacket(MsexVersion version, string productName,
			byte productVersionMajor, byte productVersionMinor, IEnumerable<DmxPatchInfo> layerDmxSources, ushort requestResponseIndex = 0)
			: base(MsexMessageType.ServerInformationMessage, version, requestResponseIndex)
		{
			Uuid = Guid.Empty;
			ProductName = productName;
			ProductVersionMajor = productVersionMajor;
			ProductVersionMinor = productVersionMinor;
			ProductVersionBugfix = 0;
			SupportedMsexVersions = ImmutableHashSet<MsexVersion>.Empty;
			SupportedLibraryTypes = ImmutableHashSet<MsexLibraryType>.Empty;
			ThumbnailFormats = ImmutableHashSet<MsexImageFormat>.Empty;
			StreamFormats = ImmutableHashSet<MsexImageFormat>.Empty;
			LayerDmxSources = layerDmxSources.ToImmutableList();
		}

		public ServerInformationPacket(MsexVersion version, Guid uuid, string productName,
			byte productVersionMajor, byte productVersionMinor, byte productVersionBugfix,
			IEnumerable<MsexVersion> supportedMsexVersions, IEnumerable<MsexLibraryType> supportedLibraryTypes,
			IEnumerable<MsexImageFormat> thumbnailFormats, IEnumerable<MsexImageFormat> streamFormats,
			IEnumerable<DmxPatchInfo> layerDmxSources, ushort requestResponseIndex = 0)
			: base(MsexMessageType.ServerInformationMessage, version, requestResponseIndex)
		{
			Uuid = uuid;
			ProductName = productName;
			ProductVersionMajor = productVersionMajor;
			ProductVersionMinor = productVersionMinor;
			ProductVersionBugfix = productVersionBugfix;
			SupportedMsexVersions = supportedMsexVersions.ToImmutableHashSet();
			SupportedLibraryTypes = supportedLibraryTypes.ToImmutableHashSet();
			ThumbnailFormats = thumbnailFormats.ToImmutableHashSet();
			StreamFormats = streamFormats.ToImmutableHashSet();
			LayerDmxSources = layerDmxSources.ToImmutableList();
		}

		public Guid Uuid { get; private set; }

		public string ProductName { get; private set; }

		public byte ProductVersionMajor { get; private set; }
		public byte ProductVersionMinor { get; private set; }
		public byte ProductVersionBugfix { get; private set; }

		public ImmutableHashSet<MsexVersion> SupportedMsexVersions { get; private set; }

		public ImmutableHashSet<MsexLibraryType> SupportedLibraryTypes { get; private set; }

		public ImmutableHashSet<MsexImageFormat> ThumbnailFormats { get; private set; }
		public ImmutableHashSet<MsexImageFormat> StreamFormats { get; private set; }

		public ImmutableList<DmxPatchInfo> LayerDmxSources { get; private set; }

		protected override void SerializeToStream(CitpBinaryWriter writer)
		{
			base.SerializeToStream(writer);

			if (Version == MsexVersion.Version1_2)
				writer.Write(Uuid);

			writer.Write(ProductName);
			writer.Write(ProductVersionMajor);
			writer.Write(ProductVersionMinor);

			if (Version == MsexVersion.Version1_2)
			{
				writer.Write(ProductVersionBugfix);

				writer.Write(SupportedMsexVersions, TypeCode.Byte, v => writer.Write(v, true));

				ushort supportedLibraryTypes = 0;
				foreach (var t in SupportedLibraryTypes)
					supportedLibraryTypes |= (ushort)(2 ^ (int)t);
				writer.Write(supportedLibraryTypes);

				writer.Write(ThumbnailFormats, TypeCode.Byte, f => writer.Write(f.GetCustomAttribute<CitpId>().Id));
				writer.Write(StreamFormats, TypeCode.Byte, f => writer.Write(f.GetCustomAttribute<CitpId>().Id));
			}

			writer.Write(LayerDmxSources, TypeCode.Byte, d => writer.Write(d, true));
		}

		protected override void DeserializeFromStream(CitpBinaryReader reader)
		{
			base.DeserializeFromStream(reader);

			if (Version == MsexVersion.Version1_2)
				Uuid = reader.ReadGuid();

			ProductName = reader.ReadString();
			ProductVersionMajor = reader.ReadByte();
			ProductVersionMinor = reader.ReadByte();

			if (Version == MsexVersion.Version1_2)
			{
				ProductVersionBugfix = reader.ReadByte();

				SupportedMsexVersions =
					reader.ReadCollection(TypeCode.Byte, () => reader.ReadMsexVersion(true)).ToImmutableHashSet();

				var supportedLibraryTypesList = new List<MsexLibraryType>();
				var supportedLibraryTypesBits = new BitArray(reader.ReadBytes(2));
				for (byte i = 0; i < supportedLibraryTypesBits.Length; ++i)
				{
					if (supportedLibraryTypesBits[i])
						supportedLibraryTypesList.Add((MsexLibraryType)i);
				}
				SupportedLibraryTypes = supportedLibraryTypesList.ToImmutableHashSet();

				ThumbnailFormats = reader.ReadCollection(TypeCode.Byte,
					() => CitpEnumHelper.GetEnumFromIdString<MsexImageFormat>(reader.ReadIdString()))
					.ToImmutableHashSet();

				StreamFormats = reader.ReadCollection(TypeCode.Byte,
					() => CitpEnumHelper.GetEnumFromIdString<MsexImageFormat>(reader.ReadIdString()))
					.ToImmutableHashSet();
			}

			LayerDmxSources = reader.ReadCollection(TypeCode.Byte,
					   () => DmxPatchInfo.Parse(reader.ReadString(true))).ToImmutableList();
		}
	}
}