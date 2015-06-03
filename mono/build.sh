pushd ..
scons libws2811.so
popd
mkdir ./lib -p
cp ../libws2811.so -t ./lib
xbuild