using SDL2;

namespace EmuInvaders.Emulator
{
    internal class SDLException : Exception
    {
        public SDLException(string message) : base($"{message} SDL_Error: {SDL.SDL_GetError()}")
        {
        }
    }
}
