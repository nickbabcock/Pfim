using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pfim
{
    public interface IImage
    {
        byte[] Data { get; }
        int Width { get; }
        int Height { get; }
        int Stride { get; }
        ImageFormat Format { get; }
    }
}
