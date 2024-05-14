using System;

namespace Pixsper.CitpSharp
{
    internal class StreamRequest : IEquatable<StreamRequest>
    {
        public StreamRequest(PeerInfo peer, MsexVersion version, MsexImageFormat format, ushort width, ushort height,
            byte fps)
        {
            Peer = peer;
            Version = version;
            Format = format;
            Width = width;
            Height = height;
            Fps = fps;
        }

        public PeerInfo Peer { get; }
        public MsexVersion Version { get; }
        public MsexImageFormat Format { get; }
        public ushort Width { get; }
        public ushort Height { get; }
        public byte Fps { get; }

        public bool Equals(StreamRequest? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Peer.Equals(other.Peer) && Version == other.Version && Format == other.Format && Width == other.Width
                   && Height == other.Height && Fps == other.Fps;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((StreamRequest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Peer.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Version;
                hashCode = (hashCode * 397) ^ (int)Format;
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                hashCode = (hashCode * 397) ^ Fps.GetHashCode();
                return hashCode;
            }
        }
    }
}