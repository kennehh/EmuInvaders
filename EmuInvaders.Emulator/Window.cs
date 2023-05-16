using EmuInvaders.Machine;
using System.Collections;
using System.Diagnostics;
using static SDL2.SDL;

namespace EmuInvaders.Emulator
{
    internal class Window : IDisposable
    {
        private const int RenderWidth = 224;
        private const int RenderHeight = 256;
        private const int InitialWindowWidth = RenderWidth * 3;
        private const int InitialWindowHeight = RenderHeight * 3;
        private const int TargetHz = 60;

        private nint window = nint.Zero;
        private nint renderer = nint.Zero;

        private SpaceInvadersMachine machine = null;
        private Thread emulatorThread = null;

        private bool quit = false;
        private int windowWidth = InitialWindowWidth;
        private int windowHeight = InitialWindowHeight;

        private Stopwatch timer = new Stopwatch();

        public Window()
        {
            machine = new SpaceInvadersMachine();
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

            window = SDL_CreateWindow("EmuInvaders", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, windowWidth, windowHeight, SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
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
                timer.Start();

                PollForEvents();
                Render();

                if (timer.Elapsed.TotalMilliseconds < (1000 / TargetHz))
                {
                    var delay = (1000 / TargetHz) - timer.Elapsed.TotalMilliseconds;
                    SDL_Delay((uint)delay);
                }

                timer.Reset();
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
            }
        }

        private void HandleKey(SDL_EventType keyEvent, SDL_Keycode keycode)
        {
            if (keyEvent != SDL_EventType.SDL_KEYDOWN && keyEvent != SDL_EventType.SDL_KEYUP)
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
            SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL_RenderClear(renderer);

            var bits = new BitArray(machine.GetFrameBuffer());
            var i = 0;

            for (var x = 0; x < RenderWidth; x++)
            {
                for (var y = RenderHeight - 1; y >= 0; y--)
                {
                    if (bits[i++])
                    {
                        if (y >= 184 && y <= 223 || y >= 238 && y <= 240 || y >= 238 && x >= 20 && x <= 60)
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
        }

        public void Dispose()
        {
            machine.Stop();

            SDL_DestroyRenderer(renderer);
            SDL_DestroyWindow(window);
            SDL_Quit();
        }
    }
}
