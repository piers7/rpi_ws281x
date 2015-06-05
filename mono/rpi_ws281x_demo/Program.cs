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
        const int defaultLedCount                          = (WIDTH * HEIGHT);

        static volatile int _cancelRequested;

        static int Main(string[] args)
        {
            // For real use probably want to hook more kill signals than this
            Console.TreatControlCAsInput = true;

            var ledCount = (args.Length > 0)
                ? int.Parse(args[0])
                : defaultLedCount
            ;
            using (var client = Ws281xClient.Create(ledCount, GPIO_PIN))
            {
                Console.WriteLine("Running demo on pin {0}, {1} leds", client.GpioPin, client.PixelCount);
                for (var i = 0; i < ledCount; i++)
                {
                    if (IsCancelRequested())
                        return 0;

                    // simple R/G/B pattern for testing
                    uint color = 0;
                    switch (i % 3)
                    {
                        case 0:
                            color = 0x00FF0000;
                            break;
                        case 1:
                            color = 0x0000FF00;
                            break;
                        case 2:
                            color = 0x00000FF;
                            break;
                    }
                    // var color = Ws281xClient.Wheel(i, 0, ledCount);
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
