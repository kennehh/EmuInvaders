using EmuInvaders.Cpu;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace EmuInvaders.Machine
{
    public class SpaceInvadersMachine
    {
        public ControlPanel Keyboard { get; } = new ControlPanel();
        public Audio Audio { get; } = new Audio();

        private const int HardwareHz = 60;
        private const int CpuSpeedHz = 2000000; // 2MHz
        private const int CpuTicksPerMillisecond = CpuSpeedHz / 1000;
        private const int ExpectedMillisecondsPerInterrupt = 1000 / (HardwareHz * 2);
        private const int ExpectedCpuCyclesPerInterrupt = ExpectedMillisecondsPerInterrupt * CpuTicksPerMillisecond;

        private readonly Intel8080 cpu = new Intel8080();
        private readonly ShiftRegister shiftRegister = new ShiftRegister();

        private readonly Stopwatch timer = new Stopwatch();
        private int nextInterrupt = 0x08;
        private bool stop = false;

        public void Initialise(string romFolder = "rom")
        {
            InitialiseInputs();
            InitialiseOutputs();
            LoadRom(romFolder);
            cpu.State.Memory.SetFrameBufferRegion(0x2400, 0x4000);
        }

        public void Run()
        {
            int cycles = 0;

            while (!stop)
            {
                timer.Start();

                if (cpu.State.InterruptsEnabled)
                {
                    GenerateInterrupt();
                }

                while (cycles < ExpectedCpuCyclesPerInterrupt)
                {
                    cycles += cpu.Step();
                }

                var timeElapsed = timer.ElapsedMilliseconds;
                if (timeElapsed < ExpectedMillisecondsPerInterrupt)
                {
                    var sleep = ExpectedMillisecondsPerInterrupt - timeElapsed;
                    Thread.Sleep((int)sleep);
                }

                cycles -= ExpectedCpuCyclesPerInterrupt;
                timer.Reset();
            }
        }

        public void Stop() => stop = true;

        public byte[] GetFrameBuffer() => cpu.State.Memory.FrameBuffer.ToArray();

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
            cpu.ConnectInputDevice(0, () => 0b0001110);

            // Input Controls
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

            // Sound 1
            cpu.ConnectOutputDevice(3, Audio.WritePort3);

            // Shift data
            cpu.ConnectOutputDevice(4, shiftRegister.Write);

            // Sound 2
            cpu.ConnectOutputDevice(5, Audio.WritePort5);
        }

        private void LoadRom(string romFolder)
        {
            cpu.LoadRom(Path.Combine(romFolder, "invaders.h"), 0x0000);
            cpu.LoadRom(Path.Combine(romFolder, "invaders.g"), 0x0800);
            cpu.LoadRom(Path.Combine(romFolder, "invaders.f"), 0x1000);
            cpu.LoadRom(Path.Combine(romFolder, "invaders.e"), 0x1800);
        }
    }
}
