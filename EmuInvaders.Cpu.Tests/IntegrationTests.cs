using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Diagnostics;
using System.Text;

namespace EmuInvaders.Cpu.Tests
{
    public class InntegrationTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("cpudiag.bin", 10, "CPU IS OPERATIONAL")]
        [TestCase("TEST.COM", 10, "CPU IS OPERATIONAL")]
        [TestCase("TST8080.COM", 10, "CPU IS OPERATIONAL")]
        [TestCase("CPUTEST.COM", 10, "CPU TESTS OK")]
        [TestCase("8080PRE.COM", 10, "8080 Preliminary tests complete")]
        [TestCase(
            "8080EXM.COM",
            600,
            "PASS! crc is:14474ba6",
            "PASS! crc is:9e922f9e",
            "PASS! crc is:cf762c86",
            "PASS! crc is:bb3f030c",
            "PASS! crc is:adb6460e",
            "PASS! crc is:83ed1345",
            "PASS! crc is:f79287cd",
            "PASS! crc is:e5f6721b",
            "PASS! crc is:15b5579a",
            "PASS! crc is:7f4e2501",
            "PASS! crc is:cf2ab396",
            "PASS! crc is:12b2952c",
            "PASS! crc is:9f2b23c0",
            "PASS! crc is:ff57d356",
            "PASS! crc is:92e963bd",
            "PASS! crc is:d5702fab",
            "PASS! crc is:a9c3d5cb",
            "PASS! crc is:e8864f26",
            "PASS! crc is:fcf46e12",
            "PASS! crc is:2b821d5f",
            "PASS! crc is:eaa72044",
            "PASS! crc is:10b58cee",
            "PASS! crc is:ed57af72",
            "PASS! crc is:e0d89235",
            "PASS! crc is:2b0471e9",
            "Tests complete"
        )]
        public void CpuTestRom_Positive(string testRom, int maxSeconds, params string[] expectedOutputs)
        {
            var stop = false;
            var output = string.Empty;
            var stopwatch = new Stopwatch();

            var cpu = new Intel8080();
            cpu.ConnectOutputDevice(0, x => stop = true);
            cpu.ConnectOutputDevice(1, x => output += PrintMessage(cpu));

            cpu.LoadCpuTestRom($"TestRoms/{testRom}");

            stopwatch.Start();
            while (!stop)
            {
                cpu.Step();

                if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
                {
                    Assert.Fail($"Test took longer than {maxSeconds} seconds");
                }
            }
            stopwatch.Stop();

            foreach (var expectedOutput in expectedOutputs)
            {
                Assert.That(output.Contains(expectedOutput));
            }
        }

        private static string PrintMessage(Intel8080 cpu)
        {
            if (cpu.State.C == 2)
            {
                // print a character stored in E
                var character = (char)cpu.State.E;
                Console.Write(character);
                return character.ToString();
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

                var message = Encoding.Default.GetString(characters.ToArray());
                Console.Write(message);
                return message;
            }

            return string.Empty;
        }
    }
}