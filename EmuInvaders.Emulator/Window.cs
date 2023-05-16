using EmuInvaders.Machine;
using SDL2;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
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

        private Dictionary<SoundType, nint> soundData = new Dictionary<SoundType, nint>();

        public Window()
        {
            machine = new SpaceInvadersMachine();
        }

        public void Open() 
        {
            machine.Initialise();
            emulatorThread = new Thread(machine.Run);
            emulatorThread.Start();

            if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO) < 0)
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

            var openAudio = SDL_mixer.Mix_OpenAudio(44100, AUDIO_S16LSB, 1, 512);
            if (openAudio != 0)
            {
                throw new SDLException("SDL could not open the SDL audio mixer");
            }

            var allocateChannels = SDL_mixer.Mix_AllocateChannels(3);
            if (allocateChannels == 0)
            {
                throw new SDLException("SDL could not allocate SDL audio mixer chnanels");
            }

            var soundTypes = Enum.GetValues(typeof(SoundType)).Cast<SoundType>().Where(x => ((int)x) >= 0);
            foreach (var type in soundTypes)
            {
                var sound = SDL_mixer.Mix_LoadWAV($"sound/{(int)type}.wav");
                if (sound == 0)
                {
                    throw new SDLException("SDL failed to load sound file");
                }
                soundData[type] = sound;
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
                PlaySounds();

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
                    KeyPress(keyEvent, Button.Left);
                    break;
                case SDL_Keycode.SDLK_RIGHT:
                    KeyPress(keyEvent, Button.Right);
                    break;
                case SDL_Keycode.SDLK_SPACE:
                    KeyPress(keyEvent, Button.Fire);
                    break;
                case SDL_Keycode.SDLK_c:
                    KeyPress(keyEvent, Button.Coin);
                    break;
                case SDL_Keycode.SDLK_RETURN:
                    KeyPress(keyEvent, Button.Start);
                    break;
            }
        }

        private void KeyPress(SDL_EventType eventType, Button keyCode)
        {
            if (eventType == SDL_EventType.SDL_KEYDOWN)
            {
                machine.Keyboard.ButtonDown(keyCode);
            }
            else if (eventType == SDL_EventType.SDL_KEYUP)
            {
                machine.Keyboard.ButtonUp(keyCode);
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

        private void PlaySounds()
        {
            foreach (var soundType in machine.Audio.GetSoundsToPlay())
            {
                int result = -1;

                if (soundType == SoundType.UfoEnd)
                {
                    SDL_mixer.Mix_Pause(GetAudioChannel(soundType));
                    result = 0;
                }
                else
                {
                    if (soundType == SoundType.UfoStart)
                    {
                        result = SDL_mixer.Mix_PlayChannel(GetAudioChannel(soundType), soundData[soundType], -1);
                    }
                    else
                    {
                        result = SDL_mixer.Mix_PlayChannel(GetAudioChannel(soundType), soundData[soundType], 0);
                    }
                }

                if (result == -1)
                {
                    throw new SDLException("Error playing sound effect");
                }
            }
        }

        private int GetAudioChannel(SoundType type)
        {
            switch (type)
            {
                case SoundType.UfoStart:
                case SoundType.UfoEnd:
                    return 0;
                case SoundType.FleetMovement1:
                case SoundType.FleetMovement2:
                case SoundType.FleetMovement3:
                case SoundType.FleetMovement4:
                    return 1;
                default:
                    return 2;
            }
        }

        public void Dispose()
        {
            machine.Stop();

            SDL_DestroyRenderer(renderer);
            SDL_DestroyWindow(window);

            foreach (var sound in soundData)
            {
                SDL_mixer.Mix_FreeChunk(sound.Value);
            }
            SDL_mixer.Mix_CloseAudio();

            SDL_Quit();
        }
    }
}
