using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace rpi_ws281x
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ws2811_t
    {
        private const int RPI_PWM_CHANNELS = 2;

        /// <summary>Private data for driver use</summary>
        /// <remarks>ws2811_device*</remarks>
        public System.IntPtr device;

        /// <summary>Required output frequency</summary>
        /// <remarks>uint32_t->DWORD->unsigned int</remarks>
        public uint freq;

        /// <summary>DMA number _not_ already in use</summary>
        /// <remarks>int</remarks>
        public int dmanum;

        /// <summary></summary>
        /// <remarks>ws2811_channel_t[RPI_PWM_CHANNELS]</remarks>
        [MarshalAsAttribute(UnmanagedType.ByValArray,
            SizeConst = RPI_PWM_CHANNELS,
            ArraySubType = UnmanagedType.Struct)
        ]
        public ws2811_channel_t[] channel;
    }
}
