using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace rpi_ws281x
{
    public class Ws281xClient : IDisposable
    {
        private ws2811_t _data;
        private readonly byte _defaultChannel;

        public static Ws281xClient Create(int ledCount, int gpioPin)
        {
            var setupInfo = new SetupInfo(ledCount) { GpioPin = gpioPin };
            return Create(setupInfo);
        }

        public static Ws281xClient Create(SetupInfo setupInfo)
        {
            var data = setupInfo.CreateDataStructure();
            return new Ws281xClient(data);
        }

        internal Ws281xClient(ws2811_t data)
        {
            _data = data;
            _defaultChannel = 0;

            // Have to be careful not to have a race condition here
            // Maybe create the wrapper which will do the cleandown on finallise
            // *before* the call to init?

            var ret = NativeMethods.ws2811_init(ref _data);
            if (ret != 0)
                throw new Exception(string.Format("ws2811_init failed - returned {0}", ret));
        }

        public int GpioPin
        {
            get { return _data.channel[_defaultChannel].gpionum; }
        }

        public int Brightness 
        {
            get { return _data.channel[_defaultChannel].brightness; }
            set { _data.channel[_defaultChannel].brightness = value; }
        }

        public int PixelCount
        {
            get { return _data.channel[_defaultChannel].count; }
        }

        /// <summary>
        /// Clears all pixels to black
        /// </summary>
        public void Clear()
        {
            var blank = new byte[PixelCount];
            SetPixels(blank);
        }

        /// <summary>
        /// Sets pixel <param name="n"/> from a 32 bit RGB color (0x00RRGGBB)
        /// </summary>
        public void SetPixelColor(int n, uint color)
        {
            var offset = n * 4;
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);    // r
            var g = (byte)(color >> 8);     // g
            var b = (byte)(color);          // b
            SetPixelColor(n, r, g, b);
        }

        /// <summary>
        /// Sets pixel <param name="n"/> from separate R,G,B components
        /// </summary>
        public void SetPixelColor(int n, byte r, byte g, byte b)
        {
            BoundsCheck(n, 0, PixelCount - 1, "n");

            var ptr = _data.channel[_defaultChannel].leds;

            var a = (byte)0;
            var bytes = new byte[] { b, g, r, a }; // little endian layout
            if (!BitConverter.IsLittleEndian)
                bytes = Reverse(bytes);

            // Would be nice to map all 4 bytes in one go
            // Marshal.Copy(bytes, 0, ptr, bytes.Length);
            // but the above only hits the first pixel,
            // unless we do some pointer math
            // or pad out the 'bytes' array appropriately

            // Instead, use the hammer and just marshal each byte seperately
            // As a result, Endianness is VERY important
            var offset = n * 4;
            for (int i = 0; i < bytes.Length; i++)
                Marshal.WriteByte(ptr, offset + i, bytes[i]);
            // Of course, if Marshal.WriteUInt32() existed, this wouldn't be a problem
        }

        /// <summary>
        /// Gets the color of pixel <param name="n"/> as a 32 bit RGB color (0x00RRGGBB)
        /// </summary>
        public uint GetPixelColor(int n)
        {
            BoundsCheck(n, 0, PixelCount - 1, "n");

            var offset = n * 4;
            var ptr = _data.channel[_defaultChannel].leds;

            var a = Marshal.ReadByte(ptr, offset + 0);
            var r = Marshal.ReadByte(ptr, offset + 1);
            var g = Marshal.ReadByte(ptr, offset + 2);
            var b = Marshal.ReadByte(ptr, offset + 3);

            var bytes = new byte[] { b, g, r, a }; // little endian layout
            if (!BitConverter.IsLittleEndian)
                bytes = Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Gets the entire pixel buffer, as an array of packed integers (0x00RRGGBB)
        /// </summary>
        public uint[] GetPixels()
        {
            var channel = _data.channel[_defaultChannel];
            var buffer = new byte[channel.count * 4];
            Marshal.Copy(_data.channel[_defaultChannel].leds, buffer, 0, buffer.Length);

            return FromBytes(buffer);
        }

        /// <summary>
        /// Sets the entire pixel buffer from as an array of packed integers (0x00RRGGBB)
        /// </summary>
        public void SetPixels(uint[] pixels)
        {
            var channel = _data.channel[_defaultChannel];
            var maxLength = channel.count;
            BoundsCheck(pixels.Length, 0, maxLength, "pixels.Length");

            var buffer = ToBytes(pixels);
            SetPixels(buffer);
        }

        /// <summary>
        /// Sets the entire pixel buffer as-is from the buffer provided
        /// </summary>
        public void SetPixels(byte[] pixels)
        {
            var channel = _data.channel[_defaultChannel];
            var maxLength = channel.count * 4;
            BoundsCheck(pixels.Length, 0, maxLength, "pixels.Length (bytes)");

            Marshal.Copy(pixels, 0, channel.leds, pixels.Length);
        }

        /// <summary>
        /// Renders the pixel buffer to the hardware
        /// </summary>
        public void Show()
        {
            var ret = NativeMethods.ws2811_render(ref _data);
            if (ret != 0)
                throw new Exception(string.Format("ws2811_render failed - returned {0}", ret));
        }

        #region Dispose
        public void Dispose()
        {
            Console.WriteLine("Tear down");
            Dispose(true);
        }

        private void Dispose(bool disposed)
        {
            NativeMethods.ws2811_fini(ref _data);
            GC.SuppressFinalize(this);
        }

        ~Ws281xClient()
        {
            Dispose(false);
        }
        #endregion

        private static void BoundsCheck(int value, int lowerBound, int upperBound, string paramName)
        {
            // Probably get sued by Oracle for this
            if (value < lowerBound || value > upperBound)
            {
                var errorMessage = string.Format("'{0}' must be between {1} and {2}; was {3}",
                    paramName, lowerBound, upperBound, value);
                throw new ArgumentOutOfRangeException(paramName, errorMessage);
            }
        }

        /// <summary>
        /// Wheel function to cycle color through RGB over 255 points
        /// </summary>
        /// <remarks>
        /// from https://github.com/adafruit/Adafruit_NeoPixel/blob/master/examples/buttoncycler/buttoncycler.ino
        /// </remarks> 
        public static uint Wheel(byte i)
        {
            unchecked // wrap-around overflows are fine
            {
                i = (byte)(255 - i);
                if (i < 85)
                {
                    return Color((byte)(255 - i * 3), 0, (byte)(i * 3));
                }
                else if (i < 170)
                {
                    i -= 85;
                    return Color(0, (byte)(i * 3), (byte)(255 - i * 3));
                }
                else
                {
                    i -= 170;
                    return Color((byte)(i * 3), (byte)(255 - i * 3), 0);
                }
            }
        }

        public static uint Wheel(int value, int lowerBound, int length)
        {
            // Scale to 0-255
            var scaledValue = (value * 255 / (double)length) - lowerBound;
            return Wheel((byte)scaledValue);
        }

        /// <summary>
        /// Packs r,g,b bytes into a packed (big endian) uint32 representation
        /// </summary>
        public static uint Color(byte r, byte g, byte b)
        {
            return (uint)(r << 16) | (uint)(g << 8) | b;
        }

        private static uint Color(byte[] rgb)
        {
            if(BitConverter.IsLittleEndian)
                return Color(rgb[2], rgb[1], rgb[0]);
            else
                return Color(rgb[1], rgb[2], rgb[3]);
        }

        /// <summary>
        /// Unpacks a uint32 into it's a,r,g,b components
        /// </summary>
        /// <remarks>This essentially gives a big-endian representation of the
        /// actual bits in <param name="color"/></remarks>
        public static byte[] Color(uint color)
        {
            return new byte[]{
                (byte)(color >> 24),
                (byte)(color >> 16), // r
                (byte)(color >> 8),  // g
                (byte)(color)        // b
            };
        }

        private static byte[] ToBytes(uint[] pixels)
        {
            var buffer = new byte[pixels.Length * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                var packedColor = pixels[i];
                var colorBytes = Color(packedColor);

                if (BitConverter.IsLittleEndian)
                    colorBytes = Reverse(colorBytes);

                colorBytes.CopyTo(buffer, i * 4);
            }
            return buffer;
        }

        private static uint[] FromBytes(byte[] buffer)
        {
            if (buffer.Length % 4 != 0)
                throw new ArgumentException("Can only deal with multiples of 4 bytes");

            var pixels = new uint[buffer.Length / 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                var colorBytes = new byte[4];
                buffer.CopyTo(colorBytes, i * 4);
                var pixel = Color(colorBytes);
                pixels[i] = pixel;
            }
            return pixels;
        }

        private static T[] Reverse<T>(T[] input)
        {
            var output = new T[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = input[input.Length - 1 - i];
            return output;
        }


        // Marshal.StructureToPtr
        // See http://blogs.msdn.com/b/dsvc/archive/2009/11/02/p-invoke-marshal-structuretoptr.aspx
        // for better description of 3rd parameter 'fDeleteOld'
        // However, turns out we don't need that anyway - Marshal.Copy is what I should have been using
    }
}
