using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pfim
{
    public static class Pfim
    {
        public static object FromFile(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (!File.Exists(path))
                throw new FileNotFoundException("Image does not exist.", path);

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, Util.BUFFER_SIZE))
            {
                switch (Path.GetExtension(path).ToUpper())
                {
                    case ".DDS":
                        return DDSFactory.Create(fs);
                    case ".TGA":
                    case ".TPIC":
                        return Targa.Create(fs);
                    default:
                        string error = string.Format("{0}: unrecognized file format.", path);
                        throw new ApplicationException(error);
                }
            }
        }
    }
}
