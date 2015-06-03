using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace rpi_ws281x
{
    public class Ws281xClient : IDisposable
    {
        private ws2811_t _data;

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

            // Have to be careful not to have a race condition here
            // Maybe create the wrapper which will do the cleandown on finallise
            // *before* the call to init?

            var ret = NativeMethods.ws2811_init(ref _data);
            if (ret != 0)
                throw new Exception(string.Format("ws2811_init failed - returned {0}", ret));
        }

        public int Brightness 
        { 
            get { return _data.channel[0].brightness; }
            set { _data.channel[0].brightness = value; }
        }

        public int PixelCount
        {
            get { return _data.channel[0].count; }
        }

        public void SetPixelColor(int i, uint color)
        {
            var offset = i * 4;
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color);
            SetPixelColor(i, r, g, b);
        }

        public void SetPixelColor(int i, byte r, byte g, byte b)
        {
            var offset = i * 4;
            // Might be nicer to somehow write all 4 bytes at once?
            Marshal.WriteByte(_data.channel[0].leds, offset + 0, 00);
            Marshal.WriteByte(_data.channel[0].leds, offset + 1, r);
            Marshal.WriteByte(_data.channel[0].leds, offset + 2, g);
            Marshal.WriteByte(_data.channel[0].leds, offset + 3, b);
        }

        public uint GetPixelColor(int i)
        {
            var offset = i * 4;

            var a = Marshal.ReadByte(_data.channel[0].leds, offset + 0);
            var r = Marshal.ReadByte(_data.channel[0].leds, offset + 1);
            var g = Marshal.ReadByte(_data.channel[0].leds, offset + 2);
            var b = Marshal.ReadByte(_data.channel[0].leds, offset + 3);

            var bytes = new[] { a, r, g, b };
            if (BitConverter.IsLittleEndian)
                bytes = Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        public uint[] GetPixels()
        {
            var output = new uint[PixelCount];
            Marshal.PtrToStructure(_data.channel[0].leds, output);
            return output;
        }

        public void SetPixels(uint[] pixels)
        {
            // Avoid buffer overflows
            if (pixels.Length > PixelCount)
                throw new ArgumentOutOfRangeException("pixels", "too many items in the array");

            Marshal.StructureToPtr(pixels, _data.channel[0].leds, false);
        }

        private static T[] Reverse<T>(T[] input)
        {
            var output = new T[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = input[input.Length - 1 - i];
            return output;
        }

        public void Show()
        {
            var ret = NativeMethods.ws2811_render(ref _data);
            if (ret != 0)
                throw new Exception(string.Format("ws2811_render failed - returned {0}", ret));
        }

        #region Dispose
        public void Dispose()
        {
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
    }
}
