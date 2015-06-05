using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace rpi_ws281x
{
    class Program
    {
        //const int TARGET_FREQ                            = 800000;
        const int GPIO_PIN                                 = 18;
        //const int DMA                                    = 5;
        const int defaultLedCount                          = 30;

        // static volatile int _cancelRequested;

        static uint[] colors = new uint[]{
                    0x00FFFFFF,
                    0x00FF0000,
                    0x0000FF00,
                    0x000000FF,
                };

        static int Main(string[] args)
        {
            // For real use probably want to hook more kill signals than this
            Console.TreatControlCAsInput = true;

            var ledCount = (args.Length > 0)
                ? int.Parse(args[0])
                : defaultLedCount
            ;
            Console.WriteLine(@"Running on {0}
Memory layout: {1}",
                Environment.OSVersion,
                BitConverter.IsLittleEndian ? "LittleEndian" : "BigEndian"
            );
            using (var client = Ws281xClient.Create(ledCount, GPIO_PIN))
            {
                Console.WriteLine("Running demo on pin {0}, {1} leds", client.GpioPin, client.PixelCount);

                while (RunTestPatterns(client))
                {
                    // keeps going until cancelled
                }
            }
            return 0;
        }

        public static bool RunTestPatterns(Ws281xClient client)
        {
            // Test pattern using SetPixelColor(n,r,g,b)
            bool completedOk;
            completedOk = Grow(client, "SetPixelColor(n,r,g,b)", i =>
            {
                switch (i % 3)
                {
                    case 0:
                        client.SetPixelColor(i, 255, 0, 0);
                        break;
                    case 1:
                        client.SetPixelColor(i, 0, 255, 0);
                        break;
                    case 2:
                        client.SetPixelColor(i, 0, 0, 255);
                        break;
                }
            });
            if (!completedOk) return completedOk;

            // Test pattern using SetPixelColor(n, color)
            completedOk = Grow(client, "SetPixelColor(n,color)", i =>
            {
                uint color = colors[i % colors.Length];
                client.SetPixelColor(i, color);
            });
            if (!completedOk) return completedOk;

            // Test pattern using SetPixels(buffer)
            var pixels = new uint[client.PixelCount];
            completedOk = Grow(client, "SetPixels(buffer)", i =>
            {
                uint color = colors[i % colors.Length];
                pixels[i] = color;
                client.SetPixels(pixels);
            });
            if (!completedOk) return completedOk;

            return true;
        }

        /// <summary>
        /// Test pattern using SetPixelColor(i, r, g, b)
        /// </summary>
        private static bool Grow(Ws281xClient client, string name, Action<int> setPixel)
        {
            Console.WriteLine(name);
            client.Clear();
            for (var i = 0; i < client.PixelCount; i++)
            {
                if (IsCancelRequested()) return false;

                setPixel(i);
                client.Show();

                if (IsCancelRequested()) return false;
                Thread.Sleep(100);
            }

            return true;
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
