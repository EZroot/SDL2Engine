# Using CMAKE / MAKE to build SDL2, SDL2Mixer, CIMGUI

After you build the libs, ensure the correct path and dll names in your .csproj

# SDL2 - TODO

# CIMGUI - TODO

# SDL2_Image - TODO

# SDL2_Mixer
https://github.com/libsdl-org/SDL_mixer/releases

tar -xvzf SDL2_mixer-2.8.0.tar.gz 
cd SDL2_mixer-2.8.0
mkdir build
cd build
cmake ..
make

If you get errors from missing includes:

sudo apt install \                
    libsdl2-dev \
    libogg-dev \
    libvorbis-dev \
    libopusfile-dev \
    libmpg123-dev \
    libflac-dev \
    libmodplug-dev \
    libxmp-dev \
    libfluidsynth-dev \
    libwavpack-dev
