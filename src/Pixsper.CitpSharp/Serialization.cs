using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pixsper.CitpSharp
{
	internal class CitpBinaryWriter : BinaryWriter
	{
		public CitpBinaryWriter(Stream output)
			: base(output, Encoding.Unicode) { }

		public override void Write(string? value)
		{
			Write(value, false);
		}

		public void Write(string? value, bool isUtf8)
		{
			Write(isUtf8
				? Encoding.UTF8.GetBytes((value ?? string.Empty) + "\0")
				: Encoding.Unicode.GetBytes((value ?? string.Empty) + "\0"));
		}

		public void Write(Guid value)
		{
			Write(Encoding.UTF8.GetBytes(value.ToString("D")));
		}

	    public void Write(MsexLibraryId id, MsexVersion version)
	    {
	        if (version == MsexVersion.Version1_0)
	        {
	            if (!id.IsMsexV1Compatible)
	                throw new InvalidOperationException("Library id is not compatible with MSEX V1.0");

	            Write(id.MsexV1LibraryNumber);
	        }
	        else
	        {
				Write(id.Level);
				Write(id.SubLevel1);
				Write(id.SubLevel2);
				Write(id.SubLevel3);
			}
	    }

		public void Write(MsexVersion version, bool isMinorFirst)
		{
			byte major, minor;

			switch (version)
			{
				case MsexVersion.Version1_0:
					major = 1;
					minor = 0;
					break;
				case MsexVersion.Version1_1:
					major = 1;
					minor = 1;
					break;
				case MsexVersion.Version1_2:
					major = 1;
					minor = 2;
					break;
				case MsexVersion.UnsupportedVersion:
					throw new ArgumentException("Cannot serialize unknown Msex Version", nameof(version));
				default:
					throw new ArgumentOutOfRangeException(nameof(version), version, null);
			}

			if (isMinorFirst)
			{
				Write(minor);
				Write(major);
			}
			else
			{
				Write(major);
				Write(minor);
			}
		}

		public void Write<T>(ICollection<T> collection, TypeCode countType, Action<T> serialize)
		{
			int count = collection.Count;

			switch (countType)
			{
				case TypeCode.Byte:
					if (count > byte.MaxValue)
						throw new ArgumentOutOfRangeException(nameof(collection), "Collection count is greater than count integer maximum");

					Write((byte)count);
					break;

				case TypeCode.UInt16:
					if (count > ushort.MaxValue)
						throw new ArgumentOutOfRangeException(nameof(collection), "Collection count is greater than count integer maximum");

					Write((ushort)count);
					break;

				case TypeCode.UInt32:
					Write((uint)count);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(countType), countType, "Not a valid integer typecode");
			}

			foreach (var value in collection)
				serialize(value);
		}
	}



	internal class CitpBinaryReader : BinaryReader
	{
		public CitpBinaryReader(Stream input) : base(input, Encoding.Unicode) { }

		public override string ReadString()
		{
			return ReadString(false);
		}

		public string ReadString(bool isUtf8)
		{
			var result = new StringBuilder(32);

			for (int i = 0; i < BaseStream.Length; ++i)
			{
				char c = isUtf8 ? Convert.ToChar(ReadByte()) : ReadChar();

				if (c == 0)
					break;

				result.Append(c);
			}

			return result.ToString();
		}

		public string ReadIdString()
		{
			return Encoding.UTF8.GetString(ReadBytes(4), 0, 4);
		}

		public Guid ReadGuid()
		{
			return Guid.Parse(Encoding.UTF8.GetString(ReadBytes(36), 0, 36));
		}

	    public MsexLibraryId ReadLibraryId(MsexVersion version)
	    {
	        return version == MsexVersion.Version1_0 
				? MsexLibraryId.FromMsexV1LibraryNumber((int)ReadByte()) 
				: MsexLibraryId.FromByteArray(ReadBytes(4));
	    }

	    public MsexVersion ReadMsexVersion(bool isMinorFirst)
		{
			byte major, minor;

			if (isMinorFirst)
			{
				minor = ReadByte();
				major = ReadByte();
			}
			else
			{
				major = ReadByte();
				minor = ReadByte();
			}

            return major switch
            {
                1 => minor switch
                {
                    0 => MsexVersion.Version1_0,
                    1 => MsexVersion.Version1_1,
                    2 => MsexVersion.Version1_2,
                    _ => MsexVersion.UnsupportedVersion
                },
                _ => MsexVersion.UnsupportedVersion
            };
        }

		public IEnumerable<T> ReadCollection<T>(TypeCode countType, Func<T> deserializeItem)
        {
            uint count = countType switch
            {
                TypeCode.Byte => ReadByte(),
                TypeCode.UInt16 => ReadUInt16(),
                TypeCode.UInt32 => ReadUInt32(),
                _ => throw new ArgumentOutOfRangeException(nameof(countType), countType, "Not a valid integer typecode")
            };

            for (int i = 0; i < count; ++i)
				yield return deserializeItem();
        }
	}
}
