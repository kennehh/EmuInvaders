using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Cpu
{
    public class Opcode
    {
        public string Instruction { get; }
        public byte Size { get; set; }
        public Func<CpuState, int> Execute { get; }

        public Opcode(string instruction, byte size, Func<CpuState, int> execute)
        {
            Instruction = instruction;
            Size = size;
            Execute = execute;
        }
    }
}
