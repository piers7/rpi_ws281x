#!/bin/bash
mkdir swigSrc -p
swig -outdir swigSrc -namespace rpi_ws281x -dllimport libws2811 -module ws281x -csharp ../ws2811.h
