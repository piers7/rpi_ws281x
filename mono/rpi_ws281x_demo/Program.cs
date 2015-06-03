using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace rpi_ws281x
{
    class Program
    {
        const int TARGET_FREQ                              = 800000;
        const int GPIO_PIN                                 = 18;
        const int DMA                                      = 5;

        const int WIDTH                                    = 8;
        const int HEIGHT                                   = 8;
        const int LED_COUNT                                = (WIDTH * HEIGHT);

        static volatile int _cancelRequested;

        static int Main(string[] args)
        {
            Console.TreatControlCAsInput = true;
            using (var client = Ws281xClient.Create(LED_COUNT, GPIO_PIN))
            {
                Console.WriteLine("Running demo on pin {0}, {1} leds", client.GpioPin, client.PixelCount);
                for (int i = 0; i < LED_COUNT; i++)
                {
                    if (IsCancelRequested())
                        return 0;

                    var color = Ws281xClient.Wheel((byte)i);
                    Console.WriteLine("Set LED {0} to 0x{1:x}", i, color);
                    client.SetPixelColor(i, color);
                    client.Show();

                    if (!IsCancelRequested())
                        Thread.Sleep(100);
                }
            }
            return 0;
        }

        private static bool IsCancelRequested()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        return true;
                    case ConsoleKey.C:
                        return key.Modifiers == ConsoleModifiers.Control;
                }
            }
            return false;
        }
    }
}
