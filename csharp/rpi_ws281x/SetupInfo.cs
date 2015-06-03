using System;
using System.Collections.Generic;
using System.Text;

namespace rpi_ws281x
{
    public class SetupInfo
    {
        int _ledCount;

        public SetupInfo(int ledCount)
        {
            _ledCount = ledCount;

            Frequency = 800000;
            Dma = 5;
            GpioPin = 18;
            Invert = false;
            Brightness = 255;
        }

        public int LedCount { get { return _ledCount; } }
        public uint Frequency { get; set; }
        public int Dma { get; set; }
        public int GpioPin { get; set; }
        public bool Invert { get; set; }
        public byte Brightness { get; set; }

        internal ws2811_t CreateDataStructure()
        {
            var data = new ws2811_t
            {
                freq = Frequency,
                dmanum = Dma,
                channel = new ws2811_channel_t[2] {
                        new ws2811_channel_t {
                            gpionum = GpioPin,
                            count = LedCount,
                            invert = Invert ? 1 : 0,
                            brightness = Brightness,
                        },
                        new ws2811_channel_t {
                        },
                    }
            };

            return data;
        }
    }
}
