namespace Pfim
{
    public class PfimConfig
    {
        public PfimConfig(
            int bufferSize = 0x8000,
            ImageFormat? targetFormat = null,
            bool decompress = true)
        {
            BufferSize = bufferSize;
            TargetFormat = targetFormat;
            Decompress = decompress;
        }

        public int BufferSize { get; }
        public ImageFormat? TargetFormat { get; }
        public bool Decompress { get; }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == this.GetType() && Equals((PfimConfig) obj);
        }

        protected bool Equals(PfimConfig other)
        {
            return BufferSize == other.BufferSize && TargetFormat == other.TargetFormat && Decompress == other.Decompress;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = BufferSize;
                hashCode = (hashCode * 397) ^ TargetFormat.GetHashCode();
                hashCode = (hashCode * 397) ^ Decompress.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{nameof(BufferSize)}: {BufferSize}, {nameof(TargetFormat)}: {TargetFormat}, {nameof(Decompress)}: {Decompress}";
        }
    }
}
