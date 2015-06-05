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

        public int GpioPin { get { return _data.channel[_defaultChannel].gpionum; } }

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
        /// Sets pixel <param name="n"/> from a 32 bit RGB color (0x00RRGGBB)
        /// </summary>
        public void SetPixelColor(int n, uint color)
        {
            var offset = n * 4;
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color);
            SetPixelColor(n, r, g, b);
        }

        /// <summary>
        /// Sets pixel <param name="n"/> from separate R,G,B components
        /// </summary>
        public void SetPixelColor(int n, byte r, byte g, byte b)
        {
            BoundsCheck(n, 0, PixelCount - 1, "n");

            var offset = n * 4;
            var ptr = _data.channel[_defaultChannel].leds;
            // Might be nicer to somehow write all 4 bytes at once?
            Marshal.WriteByte(ptr, offset + 0, 00);
            Marshal.WriteByte(ptr, offset + 1, r);
            Marshal.WriteByte(ptr, offset + 2, g);
            Marshal.WriteByte(ptr, offset + 3, b);
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

            var bytes = new[] { a, r, g, b };
            if (BitConverter.IsLittleEndian)
                bytes = Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Gets the entire pixel buffer, as an array of packed integers (0x00RRGGBB)
        /// </summary>
        public uint[] GetPixels()
        {
            var output = new uint[PixelCount];
            Marshal.PtrToStructure(_data.channel[_defaultChannel].leds, output);
            return output;
        }

        /// <summary>
        /// Sets the entire pixel buffer from as an array of packed integers (0x00RRGGBB)
        /// </summary>
        public void SetPixels(uint[] pixels)
        {
            // Avoid buffer overflows
            if (pixels.Length > PixelCount)
                throw new ArgumentOutOfRangeException("pixels", "too many items in the array");

            Marshal.StructureToPtr(pixels, _data.channel[_defaultChannel].leds, false);
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

        public static uint Color(byte r, byte g, byte b)
        {
            return (uint)(r << 16) | (uint)(g << 8) | b;
        }

        private static T[] Reverse<T>(T[] input)
        {
            var output = new T[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = input[input.Length - 1 - i];
            return output;
        }
    }
}
