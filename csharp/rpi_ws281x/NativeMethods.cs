using System;
using System.Runtime.InteropServices;

// Generated using the PInvoke Interop Assistant
// http://clrinterop.codeplex.com/releases/view/14120
// (after some encoragement, 'cause it doesn't understand stdint.h types)

namespace rpi_ws281x
{
    internal partial class NativeMethods
    {
        /// <summary>Initialize buffers/hardware</summary>
        /// <remarks>
        /// Return Type: int
        /// ws2811: ws2811_t*
        /// </remarks>
        [DllImportAttribute("libws2811", EntryPoint = "ws2811_init")]
        public static extern int ws2811_init(ref ws2811_t ws2811);


        /// <summary>Tear it all down</summary>
        /// <remarks>
        /// Return Type: void
        /// ws2811: ws2811_t*
        /// </remarks>
        [DllImportAttribute("libws2811", EntryPoint = "ws2811_fini")]
        public static extern void ws2811_fini(ref ws2811_t ws2811);


        /// <summary>Send LEDs off to hardware</summary>
        /// <remarks>
        /// Return Type: int
        /// ws2811: ws2811_t*
        /// </remarks>
        [DllImportAttribute("libws2811", EntryPoint = "ws2811_render")]
        public static extern int ws2811_render(ref ws2811_t ws2811);


        /// <summary>Wait for DMA completion</summary>
        /// <remarks>
        /// Return Type: int
        /// ws2811: ws2811_t*
        /// </remarks>
        [DllImportAttribute("libws2811", EntryPoint = "ws2811_wait")]
        public static extern int ws2811_wait(ref ws2811_t ws2811);
    }
}