# Using CMAKE / MAKE to build SDL2, SDL2Mixer, CIMGUI

After you build the libs, ensure the correct path and dll names in your .csproj

# SDL2 - TODO
 Probably similar to SDL2_Mixer
# CIMGUI - TODO
 Probably similar to SDL2_Mixer
# SDL2_Image - TODO
 Probably similar to SDL2_Mixer

# SDL2_TTF

download release tar.gz

sudo apt-get install libsdl2-dev libfreetype6-dev

tar -xzf SDL2_ttf-2.24.0.tar.gz

cd mkdir

cd build

cmake ..

make

then copy the lib to the engine /lib folder

# SDL2_Mixer
Repo: `https://github.com/libsdl-org/SDL_mixer/releases`

`tar -xvzf SDL2_mixer-2.8.0.tar.gz `

`cd SDL2_mixer-2.8.0`

`mkdir build`

`cd build`

`cmake ..`

`make`

Copy the library into the SDL2Engine /libs folder, you may have to rename the library too.
 The engine will complain about it and give you a hint.

If you get errors from missing includes:

`sudo apt install \                
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
`
