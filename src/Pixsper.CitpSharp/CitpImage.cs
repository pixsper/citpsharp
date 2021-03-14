using System;

namespace Pixsper.CitpSharp
{
    /// <summary>
    ///     Contains byte data of an image generated in response to a <see cref="CitpImageRequest" />
    /// </summary>
    public class CitpImage
    {
        /// <summary>
        ///		Maximum length of an image which can be sent via CITP
        /// </summary>
        public const int MaximumImageBufferLength = 65433;

        /// <summary>
        ///		Maximum length of an image fragment which can be sent via CITP
        /// </summary>
        public const int MaximumFragmentedImageBufferLength = 65421;

        /// <summary>
        ///		Constructs a <see cref="CitpImage"/> from a request and an image buffer
        /// </summary>
        /// <param name="request">Request used to create <see cref="ImageBuffer"/></param>
        /// <param name="imageBuffer">Byte data of requested image</param>
        /// <param name="actualWidth">Actual width of the image in <see cref="ImageBuffer"/></param>
        /// <param name="actualHeight">Actual height of the image in <see cref="ImageBuffer"/></param>
        public CitpImage(CitpImageRequest request, byte[] imageBuffer, int actualWidth, int actualHeight)
        {
            Request = request;
            ImageBuffer = imageBuffer ?? throw new ArgumentNullException(nameof(imageBuffer));

            if (actualWidth < 1 || actualWidth > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(actualWidth), actualWidth,
                    $"Must be in range 1-{ushort.MaxValue}");
            ActualWidth = actualWidth;

            if (actualHeight < 1 || actualHeight > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(actualHeight), actualHeight,
                    $"Must be in range 1-{ushort.MaxValue}");
            ActualHeight = actualHeight;

            if (!Request.IsFragmentedFormat && imageBuffer.Length > MaximumImageBufferLength)
                throw new ArgumentException(
                    $"Image too large to send via CITP. Limit is {MaximumImageBufferLength} bytes",
                    nameof(imageBuffer));
        }

        /// <summary>
        ///     The request the image was generated in response to
        /// </summary>
        public CitpImageRequest Request { get; }

        /// <summary>
        ///     The byte data of the image
        /// </summary>
        public byte[] ImageBuffer { get; }

        /// <summary>
        ///     The actual width of the image contained in <see cref="ImageBuffer" />
        /// </summary>
        public int ActualWidth { get; }

        /// <summary>
        ///     The actual height of the image contained in <see cref="ImageBuffer" />
        /// </summary>
        public int ActualHeight { get; }
    }



    /// <summary>
    ///     Represents a request for either a library/element thumbnail or streaming frame from a CITP peer.
    /// </summary>
    /// <seealso cref="CitpImage" />
    public readonly struct CitpImageRequest : IEquatable<CitpImageRequest>
    {
        internal CitpImageRequest(int frameWidth, int frameHeight, MsexImageFormat format,
            bool isPreserveAspectRatio = false,
            bool isBgrOrder = false)
        {
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            Format = format;
            IsPreserveAspectRatio = isPreserveAspectRatio;
            IsBgrOrder = isBgrOrder;
        }

        /// <summary>
        ///     The requested width for the image.
        /// </summary>
        public int FrameWidth { get; }

        /// <summary>
        ///     The requested height for the image
        /// </summary>
        public int FrameHeight { get; }

        /// <summary>
        ///     The requested format for the image
        /// </summary>
        public MsexImageFormat Format { get; }

        /// <summary>
        ///     When true, indicates that the requested image should be scaled to fit the requested width and height without
        ///     changing the image aspect ratio.
        /// </summary>
        public bool IsPreserveAspectRatio { get; }

        /// <summary>
        ///     When true, and when <see cref="Format" /> is equal to RGB, indicates that the ordering of the bytes should be BGR
        ///     rather than RGB.
        /// </summary>
        /// <remarks>This will only be true when communicating with MSEX 1.0 clients</remarks>
        public bool IsBgrOrder { get; }


        /// <summary>
        ///		True if the requested image format is a fragmented format
        /// </summary>
        public bool IsFragmentedFormat =>
            Format == MsexImageFormat.FragmentedJpeg || Format == MsexImageFormat.FragmentedPng;

        /// <inheritdoc />
        public bool Equals(CitpImageRequest other)
        {
            return FrameWidth == other.FrameWidth && FrameHeight == other.FrameHeight && Format == other.Format
                   && IsPreserveAspectRatio == other.IsPreserveAspectRatio && IsBgrOrder == other.IsBgrOrder;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is CitpImageRequest other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = FrameWidth;
                hashCode = (hashCode * 397) ^ FrameHeight;
                hashCode = (hashCode * 397) ^ (int)Format;
                hashCode = (hashCode * 397) ^ IsPreserveAspectRatio.GetHashCode();
                hashCode = (hashCode * 397) ^ IsBgrOrder.GetHashCode();
                return hashCode;
            }
        }
        
        public static bool operator ==(CitpImageRequest left, CitpImageRequest right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CitpImageRequest left, CitpImageRequest right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{FrameWidth} x {FrameHeight}"
                   + (IsPreserveAspectRatio ? " (Preserve Aspect)" : string.Empty) +
                   ", {Format}"
                   + (IsBgrOrder ? " BGR Order" : string.Empty);
        }
    }
}