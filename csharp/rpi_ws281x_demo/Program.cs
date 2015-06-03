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
            Console.CancelKeyPress += (o, e) => _cancelRequested = 1;

            using (var client = Ws281xClient.Create(LED_COUNT, GPIO_PIN))
            {
                for (int i = 0; i < LED_COUNT; i++)
                {
                    if (_cancelRequested > 0)
                        return 0;

                    Console.WriteLine("Set LED {0}", i);
                    client.SetPixelColor(i, 0x00FF0000);
                    client.Show();

                    if (_cancelRequested == 0)
                        Thread.Sleep(100);
                }
            }
            return 0;
        }
    }
}
