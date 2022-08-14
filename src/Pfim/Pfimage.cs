using System;
using System.IO;

namespace Pfim
{
    /// <summary>Decodes images into a uniform structure</summary>
    public static class Pfimage
    {
        public static IImage FromFile(string path)
        {
            return FromFile(path, new PfimConfig());
        }

        /// <summary>Constructs an image from a given file</summary>
        public static IImage FromFile(string path, PfimConfig config)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"Image does not exist: {Path.GetFullPath(path)}", path);

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, config.BufferSize))
            {
                return FromStream(fs, config);
            }
        }

        public static IImage FromStream(Stream stream)
        {
            return FromStream(stream, new PfimConfig());
        }

        /// <summary>
        /// Create image from stream. Pfim will try to detect the format based on several leading bytes
        /// </summary>
        public static IImage FromStream(Stream stream, PfimConfig config)
        {
            byte[] magic = new byte[4];
            Util.ReadExactly(stream, magic, 0, magic.Length);
            if (magic[0] == 0x44 && magic[1] == 0x44 && magic[2] == 0x53 && magic[3] == 0x20)
            {
                return Dds.CreateSkipMagic(stream, config);
            }

            return Targa.CreateWithPartialHeader(stream, config, magic);
        }
    }
}
