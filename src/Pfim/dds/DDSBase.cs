using System;

namespace Pfim
{
    public abstract class DDSBase
    {
        protected DDSHeader Header { get; private set; } 

        private DDSLoadInfo _loadInfo;
        protected DDSLoadInfo LoadInfo
        {
            get
            {
                return _loadInfo;
            }
            set
            {
                _loadInfo = value;
                CalculateSizeInfo();
            }
        }

        public DDSBase(DDSHeader header)
        {
            Header = header;
        }
        public DDSBase(DDSHeader header, DDSLoadInfo loadinfo)
        {
            Header = header;
            LoadInfo = loadinfo;
        }

        private void CalculateSizeInfo()
        {
            Size = (int)(Math.Max(LoadInfo.divSize, Header.Width) / LoadInfo.divSize * Math.Max(LoadInfo.divSize, Header.Height) / LoadInfo.divSize * LoadInfo.blockBytes);
            //BitsPerPixel = ((int)LoadInfo.pixelFormat & 0xff00) >> 8;
            BytesPerPixel = (BitsPerPixel + 7) / 8;
            Stride = (int)(4 * ((Header.Width * BytesPerPixel + 3) / 4));
            NumberOfPixels = Header.Height * Header.Width * LoadInfo.divSize;
        }

        //public Bitmap Image { get; protected set;}

        public int Size { get; private set; }

        public int BitsPerPixel { get; private set; }

        public int BytesPerPixel { get; private set; }

        public int Stride { get; private set; }

        public uint NumberOfPixels { get; private set; }
    }
}
