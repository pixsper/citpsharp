using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Pixsper.CitpSharp
{
    public sealed class ElementLibraryUpdatedInformation : IEquatable<ElementLibraryUpdatedInformation>
    {
        public ElementLibraryUpdatedInformation(MsexLibraryType libraryType, byte libraryNumber,
            MsexLibraryId? libraryId,
            MsexElementLibraryUpdatedFlags updateFlags, IEnumerable<byte> affectedElements,
            IEnumerable<byte> affectedLibraries)
        {
            if (affectedElements == null)
                throw new ArgumentNullException(nameof(affectedElements));
            if (affectedLibraries == null)
                throw new ArgumentNullException(nameof(affectedLibraries));

            LibraryType = libraryType;
            LibraryNumber = libraryNumber;
            LibraryId = libraryId;
            UpdateFlags = updateFlags;
            AffectedElements = affectedElements.ToImmutableSortedSet();
            AffectedLibraries = affectedLibraries.ToImmutableSortedSet();
        }

        public MsexLibraryType LibraryType { get; }
        public byte LibraryNumber { get; }
        public MsexLibraryId? LibraryId { get; }

        public MsexElementLibraryUpdatedFlags UpdateFlags { get; }

        public ImmutableSortedSet<byte> AffectedElements { get; }
        public ImmutableSortedSet<byte> AffectedLibraries { get; }

        public bool Equals(ElementLibraryUpdatedInformation? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return LibraryType == other.LibraryType && LibraryNumber == other.LibraryNumber
                                                    && Nullable.Equals(LibraryId, other.LibraryId)
                                                    && UpdateFlags == other.UpdateFlags
                                                    && AffectedElements.SequenceEqual(other.AffectedElements)
                                                    && AffectedLibraries.SequenceEqual(other.AffectedLibraries);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is ElementLibraryUpdatedInformation other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)LibraryType;
                hashCode = (hashCode * 397) ^ LibraryNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ LibraryId.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)UpdateFlags;
                hashCode = (hashCode * 397) ^ AffectedElements.GetHashCode();
                hashCode = (hashCode * 397) ^ AffectedLibraries.GetHashCode();
                return hashCode;
            }
        }
    }
}