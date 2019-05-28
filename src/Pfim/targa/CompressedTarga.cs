using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Pfim
{
    /// <summary>
    /// Defines a series of functions that can decode a compressed targa image
    /// </summary>
    public class CompressedTarga : IDecodeTarga
    {
        unsafe byte[] FastPass(byte[] data, ArraySegment<byte> arr, TargaHeader header, int stride, long arrPosition)
        {
            var dataLen = header.Height * stride;
            int bytesPerPixel = header.PixelDepthBytes;

            fixed (byte* startDataPtr = data)
            fixed (byte* fixedInputPtr = &arr.Array[arrPosition])
            {
                byte* endSentinal = startDataPtr + dataLen;
                byte* dataPtr = endSentinal - stride;
                byte* inputPtr = fixedInputPtr;
                while (dataPtr >= startDataPtr)
                {
                    int pixelIndex = 0;
                    do
                    {
                        bool isRunLength = (*inputPtr & 128) != 0;
                        if (isRunLength)
                        {
                            var run = *inputPtr++ - 127;

                            if (bytesPerPixel == 3)
                            {
                                RunLength3(dataPtr, run, *inputPtr++, *inputPtr++, *inputPtr++);
                            }
                            else if (bytesPerPixel == 4)
                            {
                                byte color0 = *inputPtr++;
                                byte color1 = *inputPtr++;
                                byte color2 = *inputPtr++;
                                byte color3 = *inputPtr++;
                                var comb = (color0 | color1 << 8 | color2 << 16 | color3 << 24);
                                Util.memset((int*)dataPtr, comb, run * 4);
                            }
                            else if (bytesPerPixel == 1)
                            {
                                Util.memset(dataPtr, *inputPtr++, run);
                            }
                            else if (bytesPerPixel == 2)
                            {
                                byte color0 = *inputPtr++;
                                byte color1 = *inputPtr++;
                                var comb = color0 | color1 << 8 | color0 << 16 | color1 << 24;
                                Util.memset((int*)dataPtr, comb, run * 2);
                            }

                            dataPtr += run * bytesPerPixel;
                            pixelIndex += run;
                        }
                        else
                        {
                            int pixels = *inputPtr++ + 1;
                            int bytes = pixels * bytesPerPixel;
                            Buffer.MemoryCopy(inputPtr, dataPtr, endSentinal - dataPtr, bytes);
                            dataPtr += bytes;
                            pixelIndex += pixels;
                            inputPtr += bytes;
                        }
                    } while (pixelIndex < header.Width);
                    dataPtr -= bytesPerPixel * header.Width + stride;
                }
            }

            return data;
        }

        /// <summary>Fills data starting from the bottom left</summary>
        public byte[] BottomLeft(Stream str, TargaHeader header, PfimConfig config)
        {
            var stride = Util.Stride(header.Width, header.PixelDepthBits);
            var dataLen = header.Height * stride;
            var data = config.Allocator.Rent(dataLen);

            if (str is MemoryStream s && s.TryGetBuffer(out var arr))
            {
                return FastPass(data, arr, header, stride, s.Position);
            }

            int dataIndex = dataLen - stride;
            int bytesPerPixel = header.PixelDepthBytes;
            int fileBufferIndex = 0;

            // Calculate the maximum number of bytes potentially needed from the buffer.
            // If our buffer doesn't have enough to decode the maximum number of bytes,
            // fetch another batch of bytes from the stream.
            int maxRead = bytesPerPixel * 128 + 1;

            byte[] filebuffer = config.Allocator.Rent(config.BufferSize);
            try
            {
                int workingSize = str.Read(filebuffer, 0, config.BufferSize);
                while (dataIndex >= 0)
                {
                    int colIndex = 0;
                    do
                    {
                        if (config.BufferSize - fileBufferIndex < maxRead && workingSize == config.BufferSize)
                        {
                            workingSize = Util.Translate(str, filebuffer, config.BufferSize, fileBufferIndex);
                            fileBufferIndex = 0;
                        }

                        bool isRunLength = (filebuffer[fileBufferIndex] & 128) != 0;
                        int count = isRunLength ? bytesPerPixel + 1 : filebuffer[fileBufferIndex] + 1;

                        // If the first bit is on, it means that the next packet is run length encoded
                        if (isRunLength)
                        {
                            RunLength(data, filebuffer, dataIndex, fileBufferIndex, bytesPerPixel);
                            dataIndex += (filebuffer[fileBufferIndex] - 127) * bytesPerPixel;
                            colIndex += filebuffer[fileBufferIndex] - 127;
                            fileBufferIndex += count;
                        }
                        else
                        {
                            int bytcount = count * bytesPerPixel;
                            fileBufferIndex++;

                            Buffer.BlockCopy(filebuffer, fileBufferIndex, data, dataIndex, bytcount);
                            fileBufferIndex += bytcount;
                            colIndex += count;
                            dataIndex += bytcount;
                        }
                    } while (colIndex < header.Width);
                    dataIndex -= bytesPerPixel * header.Width + stride;
                }

                return data;
            }
            finally
            {
                config.Allocator.Return(filebuffer);
            }
        }

        public byte[] BottomRight(Stream str, TargaHeader header, PfimConfig config)
        {
            return BottomLeft(str, header, config);
        }

        public byte[] TopRight(Stream str, TargaHeader header, PfimConfig config)
        {
            return BottomLeft(str, header, config);
        }

        public byte[] TopLeft(Stream str, TargaHeader header, PfimConfig config)
        {
            var stride = Util.Stride(header.Width, header.PixelDepthBits);
            var data = config.Allocator.Rent(header.Height * stride);

            int dataIndex = 0;
            int bytesPerPixel = header.PixelDepthBytes;
            int fileBufferIndex = 0;

            // Calculate the maximum number of bytes potentially needed from the buffer.
            // If our buffer doesn't have enough to decode the maximum number of bytes,
            // fetch another batch of bytes from the stream.
            int maxRead = bytesPerPixel * 128 + 1;

            byte[] filebuffer = config.Allocator.Rent(config.BufferSize);
            int workingSize = str.Read(filebuffer, 0, config.BufferSize);

            try
            {
                for (int i = 0; i < header.Height; i++)
                {
                    int colIndex = 0;
                    do
                    {
                        if (config.BufferSize - fileBufferIndex < maxRead && workingSize == config.BufferSize)
                        {
                            workingSize = Util.Translate(str, filebuffer, config.BufferSize, fileBufferIndex);
                            fileBufferIndex = 0;
                        }

                        bool isRunLength = (filebuffer[fileBufferIndex] & 128) != 0;
                        int count = isRunLength ? bytesPerPixel + 1 : filebuffer[fileBufferIndex] + 1;

                        // If the first bit is on, it means that the next packet is run length encoded
                        if (isRunLength)
                        {
                            RunLength(data, filebuffer, dataIndex, fileBufferIndex, bytesPerPixel);
                            dataIndex += (filebuffer[fileBufferIndex] - 127) * bytesPerPixel;
                            colIndex += filebuffer[fileBufferIndex] - 127;
                            fileBufferIndex += count;
                        }
                        else
                        {
                            int bytcount = count * bytesPerPixel;
                            fileBufferIndex++;

                            Buffer.BlockCopy(filebuffer, fileBufferIndex, data, dataIndex, bytcount);
                            fileBufferIndex += bytcount;
                            colIndex += count;
                            dataIndex += bytcount;
                        }
                    } while (colIndex < header.Width);
                }

                return data;
            }
            finally
            {
                config.Allocator.Return(filebuffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void RunLength3(byte* dataPtr, int runLength, byte color0, byte color1, byte color2)
        {
            // If the color depth is three, we are able to do a unique optimization.
            // Four pixels in a row is twelve bytes long. Thus we construct three ints
            // that will contain all orderings of the pixels. So when set the data equal
            // to one of these ints, we are really placing 1 (1/3) pixels. We are safe
            // using this method because the specification states that run length
            // packets cannot span more than one stride.
            int* workingptr = (int*)dataPtr;
            int size = 3 * runLength;
            int comb = (color0 | (color1 << 8) | (color2 << 16) | (color0 << 24));
            int comb1 = (color1 | (color2 << 8) | (color0 << 16) | (color1 << 24));
            int comb2 = (color2 | (color0 << 8) | (color1 << 16) | (color2 << 24));

            if (size > 12)
            {
                do
                {
                    *workingptr++ = comb;
                    *workingptr++ = comb1;
                    *workingptr++ = comb2;
                } while ((size -= 12) >= 12);
            }

            byte* byteWorkingPtr = (byte*)workingptr;
            while (size > 0)
            {
                *byteWorkingPtr++ = color0;
                *byteWorkingPtr++ = color1;
                *byteWorkingPtr++ = color2;
                size -= 3;
            }
        }

        /// <summary>
        /// Compressed Targa images contain Run Length Packets of length 1 + <paramref name="colorDepth"/>.
        /// The first byte contains how long many pixels are encoded in the packet and the following bytes
        /// determine what colors will be. This function will expand this packet and store the expansion
        /// in the data buffer.
        /// </summary>
        /// <param name="data">Where the expanded run length data will be stored.</param>
        /// <param name="streamBuffer">Buffer that contains the compressed run length info.</param>
        /// <param name="dataIndex">Index of where to start storing the expanded data.</param>
        /// <param name="streamBufferIndex">Index of where the compressed data.</param>
        /// <param name="colorDepth">The number of bytes in a pixel</param>
        public static unsafe void RunLength(byte[] data, byte[] streamBuffer,
            int dataIndex, int streamBufferIndex, int colorDepth)
        {
            int runLength = streamBuffer[streamBufferIndex++] - 127;

            fixed (byte* ptr = &data[dataIndex])
            {
                if (colorDepth == 3)
                {
                    byte color0 = streamBuffer[streamBufferIndex++];
                    byte color1 = streamBuffer[streamBufferIndex++];
                    byte color2 = streamBuffer[streamBufferIndex++];
                    RunLength3(ptr, runLength, color0, color1, color2);
                }
                else if (colorDepth == 4)
                {
                    byte color0 = streamBuffer[streamBufferIndex++];
                    byte color1 = streamBuffer[streamBufferIndex++];
                    byte color2 = streamBuffer[streamBufferIndex++];
                    byte color3 = streamBuffer[streamBufferIndex++];
                    var comb = (color0 | color1 << 8 | color2 << 16 | color3 << 24);
                    Util.memset((int*)ptr, comb, runLength * 4);
                }
                else if (colorDepth == 1)
                {
                    Util.memset(ptr, streamBuffer[streamBufferIndex++], runLength);
                }
                else if (colorDepth == 2)
                {
                    byte color0 = streamBuffer[streamBufferIndex++];
                    byte color1 = streamBuffer[streamBufferIndex++];
                    var comb = color0 | color1 << 8 | color0 << 16 | color1 << 24;
                    Util.memset((int*)ptr, comb, runLength * 2);
                }
            }
        }
    }
}
