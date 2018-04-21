#if NETSTANDARD1_3
using System;
using System.IO;

namespace Pfim
{
    /// <summary>Decodes images into a uniform structure</summary>
    public static class Pfim
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
                switch (Path.GetExtension(path).ToUpper())
                {
                    case ".DDS":
                        return Dds.Create(fs, config);
                    case ".TGA":
                    case ".TPIC":
                        return Targa.Create(fs, config);
                    default:
                        string error = string.Format("{0}: unrecognized file format.", path);
                        throw new Exception(error);
                }
            }
        }
    }
}
#endif