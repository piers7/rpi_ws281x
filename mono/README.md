
#.Net Library for WS281x

This is a .net / mono C# wrapper for the ws281x native shared library. The wrapper uses P/Invoke to call the shared library directly*

##Setup

In order to build the mono wrapper, you will need to install **SCONS** and **mono** if not already present:

    sudo apt-get install scons mono-dev

## Build

Run the `./build.sh` script. This uses SCONS to build the native shared library, and xbuild to build the C# library and demo app.

## Test app

Run the `./runDemo.sh` script. This run the rpi\_ws281x_demo.exe soak test that I used to find all the bugs.

## Using the Library
The library is intended to be relatively familiar to anyone who's used the Adafruit NeoPixel libraries:

	using (var client = Ws281xClient.Create(ledCount, GPIO_PIN))
	{
		// set pixel 1 from RGB bytes
		client.SetPixelColor(1, 255,120,0);

		// set pixel 2 from a packed int (using hex literal)
		client.SetPixelColor(2, 0xFFEE00);

		// render the buffer
		client.Show();

		// etc...
	}

Additionally you can use the overloads of SetPixels() that allow you to specify the contents of the entire pixel buffer in one hit (either directly, or as a map function), and of course you can retrieve pixel values with GetPixelColor().

Note the `using` statement - the client is `IDisposable` and should be disposed in order to tear down the PWM/DMA at the end of the session. Note: for completeness this requires hooking more signals than the demo app currently does.

<br/>
<small>
\* I did look at using SWIG, but as far as I can tell the CLR doesn't really need it, and can manage all the interop / marshalling / pointer handling directly.
</small>

/piers7

