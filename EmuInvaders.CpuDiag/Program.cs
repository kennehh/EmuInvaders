using EmuInvaders.Cpu;
using System;
using System.Diagnostics;
using System.Text;

internal class Program
{
    private static bool stop = false;
    private static Intel8080 cpu = new Intel8080();

    private static void Main(string[] args)
    {
        cpu = new Intel8080();
        //cpu.LoadCpuTestRom("TEST.COM");
        //cpu.LoadCpuTestRom("cpudiag.bin");
        //cpu.LoadCpuTestRom("CPUTEST.COM");
        //cpu.LoadCpuTestRom("TST8080.COM");
        //cpu.LoadCpuTestRom("8080PRE.COM");
        //cpu.LoadCpuTestRom("TEST.COM");
        cpu.LoadCpuTestRom("8080EXM.COM");
        //cpu.State.OutputHandler = new OutputHandler();
        cpu.ConnectOutputDevice(0, val => stop = true);
        cpu.ConnectOutputDevice(1, PrintMessage);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (!stop)
        {
            cpu.Step();
        }
        stopwatch.Stop();
        Console.WriteLine();
        Console.WriteLine(stopwatch.ElapsedMilliseconds);
    }

    private static void PrintMessage(byte value)
    {
        if (cpu.State.C == 2)
        {
            // print a character stored in E
            Console.Write((char)cpu.State.E);
        }
        else if (cpu.State.C == 9)
        {
            // print from memory at (DE) until '$' char
            var offset = cpu.State.DE;
            var data = cpu.State.Memory.GetSubsetOfMemory(offset, cpu.State.Memory.Length - offset).ToArray();
            var characters = new List<byte>();
            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream))
            {
                var c = reader.Read();
                while (c != '$')
                {
                    characters.Add((byte)c);
                    c = reader.Read();
                };
            }
            Console.Write(Encoding.Default.GetString(characters.ToArray()));
        }
    }
}
