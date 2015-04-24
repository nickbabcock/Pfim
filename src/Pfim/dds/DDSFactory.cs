using System;
using System.IO;

namespace Pfim
{
    internal static class DDSFactory
    {
        internal static DDSBase Create(string filePath)
        {
            using (FileStream fsStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                DDSHeader header = new DDSHeader(fsStream);
                switch (header.PixelFormat.FourCC)
                {
                    case CompressionAlgorithm.D3DFMT_DXT1:
                        return new DXT1DDS(fsStream, header);
                    case CompressionAlgorithm.D3DFMT_DXT2:
                    case CompressionAlgorithm.D3DFMT_DXT4:
                        throw new ArgumentException("Cannot support DXT2 or DXT4");
                    case CompressionAlgorithm.D3DFMT_DXT3:
                        return new DXT3DDS(fsStream, header);
                    case CompressionAlgorithm.D3DFMT_DXT5:
                        return new DXT5DDS(fsStream, header);
                    case CompressionAlgorithm.None:
                        return new UnCompressedDDS(fsStream, header);
                    default:
                        throw new ArgumentException("FourCC: " + header.PixelFormat.FourCC + " not supported.");
                }
            }
        }
    }
}
