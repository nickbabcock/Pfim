using System.IO;

namespace Pfim
{
    internal interface IDecodeTarga
    {
        byte[] BottomLeft(Stream str, TargaHeader header);

        byte[] BottomRight(Stream str, TargaHeader header);

        byte[] TopRight(Stream str, TargaHeader header);

        byte[] TopLeft(Stream str, TargaHeader header);
    }
}