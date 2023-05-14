using EmuInvaders.Cpu;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EmuInvaders.Machine
{
    public class SpaceInvadersMachine
    {
        public event EventHandler<RefreshDisplayEventArgs> Render;
        public Keyboard Keyboard { get; } = new Keyboard();

        private readonly Intel8080 cpu = new Intel8080();
        private readonly ShiftRegister shiftRegister = new ShiftRegister();
        private readonly Stopwatch timer = new Stopwatch();
        private int nextInterrupt = 0x08;
        private bool stop = false;

        public void Initialise()
        {
            InitialiseInputs();
            InitialiseOutputs();
            LoadRom();
            cpu.State.Memory.SetFrameBufferRegion(0x2400, 0x4000);
        }

        public void Run()
        {
            int cycles = 0;
            var renderNow = true;

            while (!stop)
            {
                timer.Start();

                if (cpu.State.InterruptsEnabled)
                {
                    GenerateInterrupt();
                }

                while (cycles < 16667)
                {
                    cycles += cpu.Step();
                }

                var timeElapsed = timer.ElapsedMilliseconds;
                if (timeElapsed < 1000 / 120)
                {
                    var sleep = 1000 / 120 - timeElapsed;
                    Thread.Sleep((int)sleep);
                }

                if (renderNow)
                {
                    Render(this, new RefreshDisplayEventArgs(cpu.State.Memory.FrameBuffer.ToArray()));
                    renderNow = false;
                }
                else
                {
                    renderNow = true;
                }

                cycles -= 16667;
                timer.Reset();
            }
        }

        public void Stop() => stop = true;

        private void GenerateInterrupt()
        {
            cpu.GenerateInterrupt(nextInterrupt);
            nextInterrupt = nextInterrupt == 0x08 ? 0x10 : 0x08;
        }

        private void InitialiseInputs()
        {
            //  Port 0
            //  bit 0 DIP4 (Seems to be self-test-request read at power up)
            //  bit 1 Always 1
            //  bit 2 Always 1
            //  bit 3 Always 1
            //  bit 4 Fire
            //  bit 5 Left
            //  bit 6 Right
            //  bit 7 ? tied to demux port 7 ?
            cpu.ConnectInputDevice(0, () =>
            {
                return 0b0001110;
            });

            //  Port 1
            //  bit 0 = CREDIT (1 if deposit)
            //  bit 1 = 2P start (1 if pressed)
            //  bit 2 = 1P start (1 if pressed)
            //  bit 3 = Always 1
            //  bit 4 = 1P shot (1 if pressed)
            //  bit 5 = 1P left (1 if pressed)
            //  bit 6 = 1P right (1 if pressed)
            //  bit 7 = Not connected
            cpu.ConnectInputDevice(1, () => Keyboard.InputValue);

            //  Port 2
            //  bit 0 = DIP3 00 = 3 ships  10 = 5 ships
            //  bit 1 = DIP5 01 = 4 ships  11 = 6 ships
            //  bit 2 = Tilt
            //  bit 3 = DIP6 0 = extra ship at 1500, 1 = extra ship at 1000
            //  bit 4 = P2 shot (1 if pressed)
            //  bit 5 = P2 left (1 if pressed)
            //  bit 6 = P2 right (1 if pressed)
            //  bit 7 = DIP7 Coin info displayed in demo screen 0=ON
            cpu.ConnectInputDevice(2, () => 0b00000000);

            // Shift Register data
            cpu.ConnectInputDevice(3, shiftRegister.Read);
        }

        private void InitialiseOutputs()
        {
            // Shift amount
            cpu.ConnectOutputDevice(2, shiftRegister.WriteOffset);

            // Shift data
            cpu.ConnectOutputDevice(4, shiftRegister.Write);
        }

        private void LoadRom()
        {
            cpu.LoadRom("rom/invaders.h", 0x0000);
            cpu.LoadRom("rom/invaders.g", 0x0800);
            cpu.LoadRom("rom/invaders.f", 0x1000);
            cpu.LoadRom("rom/invaders.e", 0x1800);
        }
    }

    public class RefreshDisplayEventArgs : EventArgs
    {
        public byte[] FrameBuffer { get; }

        public RefreshDisplayEventArgs(byte[] frameBuffer)
        {
            FrameBuffer = frameBuffer;
        }
    }
}
