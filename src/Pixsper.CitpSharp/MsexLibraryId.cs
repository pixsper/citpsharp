using System;

namespace Pixsper.CitpSharp
{
	/// <summary>
	///     Unique identifier for a library of MSEX elements
	/// </summary>
		public readonly struct MsexLibraryId : IEquatable<MsexLibraryId>, IComparable<MsexLibraryId>
	{
		/// <summary>
		///		The MSEX root library
		/// </summary>
		public static MsexLibraryId Root { get; } = new MsexLibraryId(0);

		/// <summary>
		///		Creates an <see cref="MsexLibraryId"/> from an MSEX V1.0 library number (for compatibility)
		/// </summary>
		/// <param name="libraryNumber"></param>
		/// <returns><see cref="MsexLibraryId"/> with <see cref="Level"/> of 1 and <see cref="SubLevel1"/> of <see cref="LibraryNumber"/></returns>
		public static MsexLibraryId FromMsexV1LibraryNumber(int libraryNumber) => new MsexLibraryId(1, libraryNumber);

		/// <summary>
		///		Creates an MsexLibraryId from an MSEX V1.0 library number (for compatibility)
		/// </summary>
		/// <param name="libraryNumber"></param>
		/// <returns><see cref="MsexLibraryId"/> with <see cref="Level"/> of 1 and <see cref="SubLevel1"/> of <see cref="LibraryNumber"/></returns>
		public static MsexLibraryId FromMsexV1LibraryNumber(byte libraryNumber) => new MsexLibraryId((byte)1, libraryNumber);

		/// <summary>
		///		Creates an MSEX library ID
		/// </summary>
		/// <param name="level"></param>
		/// <param name="subLevel1"></param>
		/// <param name="subLevel2"></param>
		/// <param name="subLevel3"></param>
		public MsexLibraryId(int level, int subLevel1 = 0, int subLevel2 = 0, int subLevel3 = 0)
			: this((byte)level, (byte)subLevel1, (byte)subLevel2, (byte)subLevel3)
		{
			
		}

		/// <summary>
		///		Creates an MSEX library ID
		/// </summary>
		/// <param name="level"></param>
		/// <param name="subLevel1"></param>
		/// <param name="subLevel2"></param>
		/// <param name="subLevel3"></param>
		public MsexLibraryId(byte level, byte subLevel1 = 0, byte subLevel2 = 0, byte subLevel3 = 0)
			: this()
		{
			if (level > 3)
				throw new ArgumentOutOfRangeException(nameof(level), level, "level must be in range 0-3");

			Level = level;
			
			SubLevel1 = Level > 0 ? subLevel1 : byte.MinValue;
			SubLevel2 = Level > 1 ? subLevel2 : byte.MinValue;
			SubLevel3 = Level > 2 ? subLevel3 : byte.MinValue;
		}

		private MsexLibraryId(MsexLibraryId other, byte? level = null, byte? subLevel1 = null, byte? subLevel2 = null, byte? subLevel3 = null)
			: this()
		{
			Level = level ?? other.Level;

			if (Level > 3)
				throw new ArgumentOutOfRangeException(nameof(level), level, "level must be in range 0-3");

			SubLevel1 = Level > 0 ? (subLevel1 ?? other.SubLevel1) : byte.MinValue;
			SubLevel2 = Level > 1 ? (subLevel2 ?? other.SubLevel2) : byte.MinValue;
			SubLevel3 = Level > 2 ? (subLevel3 ?? other.SubLevel3) : byte.MinValue;
		}

		private MsexLibraryId(MsexLibraryId other, int? level = null, int? subLevel1 = null, int? subLevel2 = null, int? subLevel3 = null)
			: this(other, (byte?)level, (byte?)subLevel1, (byte?)subLevel2, (byte?)subLevel3)
		{
			
		}

		/// <summary>
		///		Level of this library ID
		/// </summary>
		public byte Level { get; }

		/// <summary>
		///		Sub-level 1 index of this library ID
		/// </summary>
		/// <remarks>Only valid if <see cref="Level"/> > 1</remarks>
		public byte SubLevel1 { get; }

		/// <summary>
		///		Sub-level 2 index of this library ID
		/// </summary>
		/// <remarks>Only valid if <see cref="Level"/> > 2</remarks>
		public byte SubLevel2 { get; }

		/// <summary>
		///		Sub-level 3 index of this library ID
		/// </summary>
		/// <remarks>Only valid if <see cref="Level"/> == 3</remarks>
		public byte SubLevel3 { get; }

		/// <summary>
		///		Sub-level of this library at it's level
		/// </summary>
		public byte LibraryNumber
		{
			get
            {
                return Level switch
                {
                    0 => 0,
                    1 => SubLevel1,
                    2 => SubLevel2,
                    3 => SubLevel3,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
		}

		/// <summary>
		/// 	Library number of this library ID for compatibility with MSEX V1.0
		/// </summary>
		/// <returns>Will throw an exception if Level != 1</returns>
		/// <exception cref="InvalidOperationException" accessor="get">This library ID cannot be represented as an MSEX V1.0</exception>
		public byte MsexV1LibraryNumber
		{
			get
			{
				if (Level != 1)
					throw new InvalidOperationException("This library ID cannot be represented as an MSEX V1.0");

				return SubLevel1;
			}
		}

		/// <summary>
		///		Returns true if library ID is equal to <see cref="Root"/>
		/// </summary>
		public bool IsRoot => Level == 0;

		/// <summary>
		///		Returns true if library ID can be represented as an MSEX V1.0 library number
		/// </summary>
		public bool IsMsexV1Compatible => Level == 1;

		/// <summary>
		///		Returns true if library is not at the maximum <see cref="Level"/>
		/// </summary>
		public bool CanHaveChildren => Level != 3;

		public MsexLibraryId SetLevel(byte value) => new MsexLibraryId(this, level: value);
		public MsexLibraryId SetLevel(int value) => new MsexLibraryId(this, level: value);
		public MsexLibraryId SetSubLevel1(byte value) => new MsexLibraryId(this, subLevel1: value);
		public MsexLibraryId SetSubLevel1(int value) => new MsexLibraryId(this, subLevel1: value);
		public MsexLibraryId SetSubLevel2(byte value) => new MsexLibraryId(this, subLevel2: value);
		public MsexLibraryId SetSubLevel2(int value) => new MsexLibraryId(this, subLevel2: value);
		public MsexLibraryId SetSubLevel3(byte value) => new MsexLibraryId(this, subLevel3: value);
		public MsexLibraryId SetSubLevel3(int value) => new MsexLibraryId(this, subLevel3: value);


		public MsexLibraryId SetLibraryNumber(byte value)
        {
            return Level switch
            {
                0 => Root,
                1 => SetSubLevel1(value),
                2 => SetSubLevel2(value),
                3 => SetSubLevel3(value),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

		public MsexLibraryId SetLibraryNumber(int value)
        {
            return Level switch
            {
                0 => throw new InvalidOperationException("Library Id is root - cannot have a library number"),
                1 => SetSubLevel1(value),
                2 => SetSubLevel2(value),
                3 => SetSubLevel3(value),
                _ => throw new ArgumentOutOfRangeException()
            };
        }


		public bool IsChildOf(MsexLibraryId other)
		{
			return other.Level == Level - 1 &&
				(other.Level == 0
				|| (other.Level == 1 && other.SubLevel1 == SubLevel1)
				|| (other.Level == 2 && other.SubLevel1 == SubLevel1 && other.SubLevel2 == SubLevel2));
		}

		public bool IsParentOf(MsexLibraryId other)
		{
			return other.IsChildOf(this);
		}
		 
		public bool IsDescendentOf(MsexLibraryId other)
		{
			return other.Level < Level &&
				(other.Level == 0 
				||(other.Level == 1 && SubLevel1 == other.SubLevel1)
				|| (other.Level == 2 && SubLevel2 == other.SubLevel2));
		}

		public bool IsAncestorOf(MsexLibraryId other)
		{
			return other.IsDescendentOf(this);
		}

		

		public int CompareTo(MsexLibraryId other)
		{
			if (SubLevel1 < other.SubLevel1)
				return -1;

			if (SubLevel1 > other.SubLevel1)
				return 1;

			if (SubLevel2 < other.SubLevel2)
				return -1;

			if (SubLevel2 > other.SubLevel2)
				return 1;

			if (SubLevel3 < other.SubLevel3)
				return -1;

			if (SubLevel3 > other.SubLevel3)
				return 1;

			return 0;
		}

        /// <inheritdoc />
        public bool Equals(MsexLibraryId other)
        {
            return Level == other.Level && SubLevel1 == other.SubLevel1 && SubLevel2 == other.SubLevel2 && SubLevel3 == other.SubLevel3;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MsexLibraryId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Level.GetHashCode();
                hashCode = (hashCode * 397) ^ SubLevel1.GetHashCode();
                hashCode = (hashCode * 397) ^ SubLevel2.GetHashCode();
                hashCode = (hashCode * 397) ^ SubLevel3.GetHashCode();
                return hashCode;
            }
        }


        public static MsexLibraryId FromByteArray(byte[] array)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (array.Length != 4)
				throw new ArgumentException("Array is incorrect length for library id.");

			if (array[0] > 3)
				throw new ArgumentException("Invalid MsexLibraryId, level must be in range 0-3");

			return new MsexLibraryId(array[0], array[1], array[2], array[3]);
		}

		public override string ToString() => $"{{{Level},{SubLevel1},{SubLevel2},{SubLevel3}}}";

		public byte[] ToByteArray() => new[] {Level, SubLevel1, SubLevel2, SubLevel3};


		public static bool operator ==(MsexLibraryId a, MsexLibraryId b) => a.Equals(b);

		public static bool operator !=(MsexLibraryId a, MsexLibraryId b) => !a.Equals(b);

		public static bool operator <(MsexLibraryId a, MsexLibraryId b) => a.CompareTo(b) == -1;

		public static bool operator >(MsexLibraryId a, MsexLibraryId b) => a.CompareTo(b) == 1;

		public static bool operator <=(MsexLibraryId a, MsexLibraryId b) => a.CompareTo(b) != 1;

		public static bool operator >=(MsexLibraryId a, MsexLibraryId b) => a.CompareTo(b) != -1;
	}
}