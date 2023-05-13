namespace EmuInvaders.Cpu.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CpuDiagTest()
        {
            var cpu = new Intel8080();
            cpu.LoadCpuDiagRom("../Roms/cpudiag.bin");

            var start = DateTime.Now;
            while (!cpu.State.Halted && string.IsNullOrEmpty(cpu.State.CpuDiagMessage) && (DateTime.Now - start).TotalSeconds < 10)
            {
                cpu.Step();
            }

            Assert.That(cpu.State.CpuDiagMessage == "CPU IS OPERATIONAL");
        }
    }
}