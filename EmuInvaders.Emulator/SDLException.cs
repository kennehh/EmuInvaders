using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuInvaders.Emulator
{
    internal class SDLException : Exception
    {
        public SDLException(string message) : base($"{message} SDL_Error: {SDL.SDL_GetError()}")
        {
        }
    }
}
