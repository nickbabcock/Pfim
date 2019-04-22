using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Pfim
{
    /// <summary>
    /// Utility class housing methods used by multiple image decoders
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Buffer size to read data from
        /// </summary>
        private const int BUFFER_SIZE = 0x8000;

        internal static MemoryStream CreateExposed(byte[] data)
        {
            return new MemoryStream(data, 0, data.Length, false, true);
        }

        /// <summary>
        /// Takes all the bytes at and after an index and moves them to the front and fills the rest
        /// of the buffer with information from the stream.
        /// </summary>
        /// <remarks>
        /// This function is useful when the buffer doesn't have enough information to process a
        /// certain amount of information thus more information from the stream has to be read. This
        /// preserves the information that hasn't been read by putting it at the front.
        /// </remarks>
        /// <param name="str">Stream where more data will be read to fill in end of the buffer.</param>
        /// <param name="buf">The buffer that contains the data that will be translated.</param>
        /// <param name="bufIndex">
        /// Start of the translation. The value initially at this index will be the value at index 0
        /// in the buffer after translation.
        /// </param>
        /// <returns>
        /// The total number of bytes read into the buffer and translated. May be less than the
        /// buffer's length.
        /// </returns>
        public static int Translate(Stream str, byte[] buf, int bufLen, int bufIndex)
        {
            Buffer.BlockCopy(buf, bufIndex, buf, 0, bufLen - bufIndex);
            int result = str.Read(buf, bufLen - bufIndex, bufIndex);
            return result + bufLen - bufIndex;
        }

        /// <summary>
        /// Sets all the values in a buffer to a single value.
        /// </summary>
        /// <param name="buffer">Buffer that will have its values set</param>
        /// <param name="value">The value that the buffer will contain</param>
        /// <param name="length">The number of bytes to write in the buffer</param>
        public static unsafe void memset(int* buffer, int value, int length)
        {
            if (length > 16)
            {
                do
                {
                    *buffer++ = value;
                    *buffer++ = value;
                    *buffer++ = value;
                    *buffer++ = value;
                } while ((length -= 16) >= 16);
            }

            for (; length > 3; length -= 4)
                *buffer++ = value;
        }

        /// <summary>
        /// Sets all the values in a buffer to a single value.
        /// </summary>
        /// <param name="buffer">Buffer that will have its values set</param>
        /// <param name="value">The value that the buffer will contain</param>
        /// <param name="length">The number of bytes to write in the buffer</param>
        public static unsafe void memset(byte* buffer, byte value, int length)
        {
            memset((int*)buffer, value << 24 | value << 16 | value << 8 | value, length);

            int rem = length % 4;
            for (int i = 0; i < rem; i++)
                buffer[length - i - 1] = value;
        }


        public static void Fill(Stream stream, byte[] data, int dataLen, int bufSize = BUFFER_SIZE)
        {
            if (stream is MemoryStream s && s.TryGetBuffer(out var arr))
            {
                Buffer.BlockCopy(arr.Array, (int)s.Position, data, 0, dataLen);
            }
            else
            {
                InnerFill(stream, data, dataLen, bufSize);
            }
        }

        public static void InnerFillUnaligned(Stream str, byte[] buf, int bufLen, int width, int stride, int bufSize = BUFFER_SIZE)
        {
            for (int i = 0; i < bufLen; i += stride)
            {
                str.Read(buf, i, width);
            }
        }

        /// <summary>
        /// Fills the buffer all the way up with info from the stream
        /// </summary>
        /// <param name="str">Stream that will be used to fill the buffer</param>
        /// <param name="buf">Buffer that will house the information from the stream</param>
        /// <param name="bufSize">The chunk size of data that will be read from the stream</param>
        private static void InnerFill(Stream str, byte[] buf, int dataLen, int bufSize = BUFFER_SIZE)
        {
            int bufPosition = 0;
            for (int i = dataLen / bufSize; i > 0; i--)
                bufPosition += str.Read(buf, bufPosition, bufSize);
            str.Read(buf, bufPosition, dataLen % bufSize);
        }

        /// <summary>
        /// Fills a data array starting from the bottom left by reading from a stream.
        /// </summary>
        /// <param name="str">The stream to read data from</param>
        /// <param name="data">The buffer to be filled with stream data</param>
        /// <param name="rowSize">The size in bytes of each row in the stream</param>
        /// <param name="bufSize">The chunk size of data that will be read from the stream</param>
        /// <param name="stride">The number of bytes that make up a row in the data</param>
        /// <param name="buffer">The temporary buffer used to read data in</param>
        public static void FillBottomLeft(
            Stream str,
            byte[] data,
            int dataLen,
            int rowSize,
            int stride,
            byte[] buffer = null,
            int bufSize = BUFFER_SIZE)
        {
            buffer = buffer ?? new byte[BUFFER_SIZE];
            if (buffer.Length < bufSize)
            {
                throw new ArgumentException("must be longer than bufSize", nameof(buffer));
            }

            int bufferIndex = 0;
            int rowsPerBuffer = Math.Min(bufSize, dataLen) / stride;
            int dataIndex = dataLen - stride;
            int rowsRead = 0;
            int totalRows = dataLen / rowSize;
            int rowsToRead = rowsPerBuffer;

            if (rowsPerBuffer == 0)
                throw new ArgumentOutOfRangeException(nameof(rowSize), "Row size must be small enough to fit in the buffer");

            int workingSize = str.Read(buffer, 0, bufSize);
            do
            {
                for (int i = 0; i < rowsToRead; i++)
                {
                    Buffer.BlockCopy(buffer, bufferIndex, data, dataIndex, rowSize);
                    dataIndex -= stride;
                    bufferIndex += rowSize;
                }

                if (dataIndex >= 0)
                {
                    workingSize = Translate(str, buffer, bufSize, bufferIndex);
                    bufferIndex = 0;
                    rowsRead += rowsPerBuffer;
                    rowsToRead = rowsRead + rowsPerBuffer < totalRows ? rowsPerBuffer : totalRows - rowsRead;
                }
            } while (dataIndex >= 0 && workingSize != 0);
        }

        /// <summary>
        /// Calculates stride of an image in bytes given its width in pixels and pixel depth in bits
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="pixelDepth">The number of bits that represents a pixel</param>
        /// <returns>The number of bytes that consists of a single line</returns>
        public static int Stride(int width, int pixelDepth)
        {
            if (width <= 0)
                throw new ArgumentException("Width must be greater than zero", nameof(width));
            else if (pixelDepth <= 0)
                throw new ArgumentException("Pixel depth must be greater than zero", nameof(pixelDepth));

            int bytesPerPixel = (pixelDepth + 7) / 8;

            // Windows GDI+ requires that the stride be a multiple of four.
            // Even if not being used for Windows GDI+ there isn't a anything
            // bad with having extra space.
            return 4 * ((width * bytesPerPixel + 3) / 4);
        }
		
        /// <summary>
        /// Creates string representation of object in format:
        /// --- CLASS NAME ---
        /// Property = value
        /// ...
        /// --- END CLASS NAME ---
        /// </summary>
        /// <param name="obj">Object to get property description of.</param>
        /// <param name="IsSubClass">True = Adds whitespace before and after, and uses less ---.</param>
        /// <returns>String of object.</returns>
        public static string StringifyObject(object obj, bool IsSubClass = false)
        {
            StringBuilder sb = new StringBuilder();
            var classname = TypeDescriptor.GetClassName(obj);
            string tags = IsSubClass ? "--" : "---";

            if (IsSubClass)
                sb.AppendLine();

            sb.AppendLine($"{tags} {classname} {tags}");
            sb.AppendLine((IsSubClass ? "    " : "") + "PROPERTIES");
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
                sb.AppendLine((IsSubClass ? "    " : "") + $"{descriptor.Name} = {descriptor.GetValue(obj)}");

            sb.AppendLine((IsSubClass ? "    " : "") + "FIELDS");
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
                sb.AppendLine((IsSubClass ? "    " : "") + $"{descriptor.Name} = {descriptor.GetValue(obj)}");

            sb.AppendLine($"{tags} END {classname} {tags}");
            sb.AppendLine();

            if (IsSubClass)
                sb.AppendLine();

            return sb.ToString();
        }		
		
		/// <summary>
		/// Reads a number of bytes from stream at the current position and advances that number of bytes.
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <param name="Length">Number of bytes to read.</param>
		/// <returns>Bytes read from stream.</returns>
		public static byte[] ReadBytes(this Stream stream, int Length)
		{
			byte[] bytes = new byte[Length];
			stream.Read(bytes, 0, Length);
			return bytes;
		}		
		
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Used in BC6.cs, because .NET Core's Color object doesn't have FromScRgb() function,
        /// so it was replaced by FromArgb().
        /// </remarks>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte ColorFloatToByte(float val)
        {
            if (!(val > 0.0)) // Handles NaN case too
                return (0);
            else if (val <= 0.0031308)
                return ((byte)((255.0f * val * 12.92f) + 0.5f));
            else if (val < 1.0)
                return ((byte)((255.0f * ((1.055f *
                                           (float)Math.Pow((double)val, (1.0 / 2.4))) - 0.055f)) + 0.5f));
            else
                return (255);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        /// <remarks>Or just better upgrade to .NET Core >= 2.2</remarks>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
