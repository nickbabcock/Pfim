using System;
using System.IO;

namespace Pfim
{
    internal static class DdsFactory
    {
        internal static DdsBase Create(Stream stream)
        {
            DdsHeader header = new DdsHeader(stream);
            switch (header.PixelFormat.FourCC)
            {
                case CompressionAlgorithm.D3DFMT_DXT1:
                    return new Dxt1Dds(stream, header);
                case CompressionAlgorithm.D3DFMT_DXT2:
                case CompressionAlgorithm.D3DFMT_DXT4:
                    throw new ArgumentException("Cannot support DXT2 or DXT4");
                case CompressionAlgorithm.D3DFMT_DXT3:
                    return new Dxt3Dds(stream, header);
                case CompressionAlgorithm.D3DFMT_DXT5:
                    return new Dxt5Dds(stream, header);
                case CompressionAlgorithm.None:
                    return new UncompressedDDS(stream, header);
                default:
                    throw new ArgumentException("FourCC: " + header.PixelFormat.FourCC + " not supported.");
            }
        }
    }
}
