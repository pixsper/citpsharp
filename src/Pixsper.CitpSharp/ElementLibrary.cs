using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixsper.CitpSharp
{
	/// <summary>
	///		A container of MSEX elements with associated library information
	/// </summary>
		public class ElementLibrary
	{
		/// <summary>
		///		Constructs an <see cref="ElementLibrary"/>
		/// </summary>
		/// <param name="libraryType">Type of this library</param>
		/// <param name="libraryInformation">Metadata for this library</param>
		/// <param name="elements">Elements in this library, each with a unique element number</param>
		public ElementLibrary(MsexLibraryType libraryType, ElementLibraryInformation libraryInformation, IEnumerable<ElementInformation> elements)
		{
			LibraryType = libraryType;
			LibraryInformation = libraryInformation;
			Elements = elements.ToImmutableDictionary(e => e.ElementNumber);
		}

		/// <summary>
		///		Type of this library
		/// </summary>
		public MsexLibraryType LibraryType { get; }

		/// <summary>
		///		Metadata for this library
		/// </summary>
		public ElementLibraryInformation LibraryInformation { get; }

		/// <summary>
		///		Elements in this library
		/// </summary>
		public ImmutableDictionary<byte, ElementInformation> Elements { get; }
	}
}
