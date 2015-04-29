using System.IO;

namespace Pfim
{
    interface IDecodeDds
    {
        DdsLoadInfo ImageInfo(DdsHeader header);
        byte[] Decode(Stream str, DdsHeader header);
    }
}
