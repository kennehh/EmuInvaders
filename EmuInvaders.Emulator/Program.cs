using EmuInvaders.Machine;
using SDL2;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace EmuInvaders.Emulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var machine = new SpaceInvadersMachine();
            machine.Initialise();

            if (SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                throw new SDLException("SDL could not initialise");
            }

            var window = SDL_CreateWindow("Space Invaders Yo", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 224, 256, SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (window == nint.Zero)
            {
                throw new SDLException("SDL could not create the window");
            }

            var renderer = SDL_CreateRenderer(window, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            if (renderer == nint.Zero)
            {
                throw new SDLException("SDL could not create the renderer");
            }

            SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL_RenderClear(renderer);
            SDL_RenderPresent(renderer);

            machine.Render += (object source, RefreshDisplayEventArgs e) =>
            {
                SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
                SDL_RenderClear(renderer);

                var bits = new BitArray(e.FrameBuffer);
                var i = 0;

                for (var x = 0; x < 224; x++)
                {
                    for (var y = 255; y >= 0; y--)
                    {
                        if (bits[i++])
                        {
                            if (y >= 182 && y <= 223)
                            {
                                // Green - player and shields
                                SDL_SetRenderDrawColor(renderer, 0, 255, 0, 255); 
                            }
                            else if (y >= 33 && y <= 55)
                            {
                                // Red - UFOs
                                SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255); 
                            }
                            else
                            {
                                // White - Everything else
                                SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
                            }

                            SDL_RenderDrawPoint(renderer, x, y);
                        }
                    }
                }

                SDL_RenderPresent(renderer);
            };

            var emulatorThread = new Thread(machine.Run);
            emulatorThread.Start();

            var quit = false;

            while (!quit) 
            {
                while (!quit && SDL_PollEvent(out var e) > 0) 
                {
                    if (e.type == SDL_EventType.SDL_QUIT)
                    {
                        quit = true;
                    }

                    switch (e.type)
                    {
                        case SDL_EventType.SDL_QUIT:
                            quit = true;
                            break;
                        case SDL_EventType.SDL_WINDOWEVENT:
                            switch (e.window.windowEvent)
                            {
                                case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                                    quit = true;
                                    break;
                            }
                            break;
                        case SDL_EventType.SDL_KEYDOWN:
                            switch (e.key.keysym.sym)
                            {
                                case SDL_Keycode.SDLK_ESCAPE:
                                    quit = true;
                                    break;
                                case SDL_Keycode.SDLK_LEFT:
                                    machine.Keyboard.KeyDown(KeyCode.Left);
                                    break;
                                case SDL_Keycode.SDLK_RIGHT:
                                    machine.Keyboard.KeyDown(KeyCode.Right);
                                    break;
                                case SDL_Keycode.SDLK_SPACE:
                                    machine.Keyboard.KeyDown(KeyCode.Fire);
                                    break;
                                case SDL_Keycode.SDLK_c:
                                    machine.Keyboard.KeyDown(KeyCode.Coin);
                                    break;
                                case SDL_Keycode.SDLK_RETURN:
                                    machine.Keyboard.KeyDown(KeyCode.Start);
                                    break;
                            }
                            break;
                        case SDL_EventType.SDL_KEYUP:
                            switch (e.key.keysym.sym)
                            {
                                case SDL_Keycode.SDLK_LEFT:
                                    machine.Keyboard.KeyUp(KeyCode.Left);
                                    break;
                                case SDL_Keycode.SDLK_RIGHT:
                                    machine.Keyboard.KeyUp(KeyCode.Right);
                                    break;
                                case SDL_Keycode.SDLK_SPACE:
                                    machine.Keyboard.KeyUp(KeyCode.Fire);
                                    break;
                                case SDL_Keycode.SDLK_c:
                                    machine.Keyboard.KeyUp(KeyCode.Coin);
                                    break;
                                case SDL_Keycode.SDLK_RETURN:
                                    machine.Keyboard.KeyUp(KeyCode.Start);
                                    break;
                            }
                            break;
                    }
                }
            }

            SDL_DestroyRenderer(renderer);
            SDL_DestroyWindow(window);
            SDL_Quit();

            machine.Stop();
        }
    }
}