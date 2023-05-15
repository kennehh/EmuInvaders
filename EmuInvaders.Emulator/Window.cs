using EmuInvaders.Machine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace EmuInvaders.Emulator
{
    internal class Window : IDisposable
    {
        private const int RenderWidth = 224;
        private const int RenderHeight = 256;
        private const int InitialWindowWidth = RenderWidth * 3;
        private const int InitialWindowHeight = RenderHeight * 3;

        private nint window = nint.Zero;
        private nint renderer = nint.Zero;

        private SpaceInvadersMachine machine = null;
        private Thread emulatorThread = null;

        private bool quit = false;
        private int windowWidth = InitialWindowWidth;
        private int windowHeight = InitialWindowHeight;
        private bool renderNow = false;
        private bool resizeNow = false;
        private byte[] currentFrameBuffer = null;

        public Window()
        {
            machine = new SpaceInvadersMachine();
            machine.OnRender += (source, args) =>
            {
                currentFrameBuffer = args.FrameBuffer;
                renderNow = true;
            };
        }

        public void Open() 
        {
            machine.Initialise();
            emulatorThread = new Thread(machine.Run);
            emulatorThread.Start();

            if (SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                throw new SDLException("SDL could not initialise");
            }

            window = SDL_CreateWindow("Space Invaders Yo", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, windowWidth, windowHeight, SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            if (window == nint.Zero)
            {
                throw new SDLException("SDL could not create the window");
            }

            renderer = SDL_CreateRenderer(window, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            if (renderer == nint.Zero)
            {
                throw new SDLException("SDL could not create the renderer");
            }

            SDL_RenderSetLogicalSize(renderer, RenderWidth, RenderHeight);
            SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL_RenderClear(renderer);
            SDL_RenderPresent(renderer);

            Loop();
        }

        private void Loop()
        {
            while (!quit)
            {
                PollForEvents();
                Render();
            }
        }

        private void PollForEvents()
        {
            while (SDL_PollEvent(out var e) > 0)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        quit = true;
                        break;
                    case SDL_EventType.SDL_WINDOWEVENT:
                        HandleWindowEvent(e.window);
                        break;
                    case SDL_EventType.SDL_KEYDOWN:
                    case SDL_EventType.SDL_KEYUP:
                        HandleKey(e.type, e.key.keysym.sym);
                        break;
                }
            }
        }

        private void HandleWindowEvent(SDL_WindowEvent windowEvent)
        {
            switch (windowEvent.windowEvent)
            {
                case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                    quit = true;
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                    windowWidth = windowEvent.data1;
                    windowHeight = windowEvent.data2;
                    resizeNow = true;
                    renderNow = true;
                    //SDL_SetWindowSize(window, windowWidth, windowHeight);
                    //SDL_RenderPresent(renderer);
                    break;
            }
        }

        private void HandleKey(SDL_EventType keyEvent, SDL_Keycode keycode)
        {
            if (keyEvent != SDL_EventType.SDL_KEYDOWN && keyEvent == SDL_EventType.SDL_KEYDOWN)
            {
                return;
            }

            switch (keycode)
            {
                case SDL_Keycode.SDLK_ESCAPE:
                    quit = true;
                    break;
                case SDL_Keycode.SDLK_LEFT:
                    KeyPress(keyEvent, KeyCode.Left);
                    break;
                case SDL_Keycode.SDLK_RIGHT:
                    KeyPress(keyEvent, KeyCode.Right);
                    break;
                case SDL_Keycode.SDLK_SPACE:
                    KeyPress(keyEvent, KeyCode.Fire);
                    break;
                case SDL_Keycode.SDLK_c:
                    KeyPress(keyEvent, KeyCode.Coin);
                    break;
                case SDL_Keycode.SDLK_RETURN:
                    KeyPress(keyEvent, KeyCode.Start);
                    break;
            }
        }

        private void KeyPress(SDL_EventType eventType, KeyCode keyCode)
        {
            if (eventType == SDL_EventType.SDL_KEYDOWN)
            {
                machine.Keyboard.KeyDown(keyCode);
            }
            else if (eventType == SDL_EventType.SDL_KEYUP)
            {
                machine.Keyboard.KeyUp(keyCode);
            }
        }

        private void Render()
        {
            if (!renderNow)
            {
                return;
            }

            SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL_RenderClear(renderer);

            var bits = new BitArray(currentFrameBuffer);
            var i = 0;

            for (var x = 0; x < RenderWidth; x++)
            {
                for (var y = RenderHeight - 1; y >= 0; y--)
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

            if (resizeNow)
            {
                SDL_SetWindowSize(window, windowWidth, windowHeight);
                resizeNow = false;
            }

            SDL_RenderPresent(renderer);
            renderNow = false;
        }

        public void Dispose()
        {
            machine.Stop();
            emulatorThread.Join();

            SDL_DestroyRenderer(renderer);
            SDL_DestroyWindow(window);
            SDL_Quit();
        }
    }
}
