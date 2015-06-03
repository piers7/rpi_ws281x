using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace rpi_ws281x
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ws2811_channel_t
    {
        /// <summary>
        /// GPIO Pin with PWM alternate function, 0 if unused
        /// </summary>
        /// <remarks>int gpionum;</remarks>
        public int gpionum;

        /// <summary>
        /// Invert output signal
        /// </summary>
        /// <remarks>int invert;</remarks>
        public int invert;

        /// <summary>
        /// Number of LEDs, 0 if channel is unused
        /// </summary>
        /// <remarks>int count;</remarks>
        public int count;

        /// <summary>
        /// Brightness value between 0 and 255
        /// </summary>
        /// <remarks>int brightness;</remarks>
        public int brightness;

        /// <summary>
        /// Pointer to LED buffers, allocated by driver based on count
        /// each is uint32_t (0x00RRGGBB)
        /// </summary>
        /// <remarks>ws2811_led_t *leds;
        /// Going to have to use explicit marshalling to get the value back
        /// See http://stackoverflow.com/questions/1197181/how-to-marshal-a-variable-sized-array-of-structs-c-sharp-and-c-interop-help
        /// </remarks>
        public System.IntPtr leds;
    }
}
