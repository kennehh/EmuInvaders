using EmuInvaders.Cpu;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var cpu = new Intel8080();
        //cpu.LoadCpuDiagRom("cpudiag.bin");
        cpu.LoadCpuTestRom("8080EXER.COM");

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (!cpu.State.Halted)// && string.IsNullOrEmpty(cpu.State.CpuDiagMessage))
        {
            cpu.Step();
        }
        stopwatch.Stop();
        Console.WriteLine(stopwatch.ElapsedMilliseconds);
    }
}