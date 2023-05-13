using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace EmuInvaders.Cpu
{
    public class Stack
    {
        public ushort SP { get; set; } = 0;

        private Memory memory;

        public Stack(Memory memory)
        {
            this.memory = memory;
        }

        public void Push(ushort value)
        {
            memory.WriteInt16((ushort)(SP - 2), value);
            SP -= 2;
        }

        public void Push(byte high, byte low)
        {
            memory.WriteInt8((ushort)(SP - 2), high);
            memory.WriteInt8((ushort)(SP - 1), low);
            SP -= 2;
        }

        public ushort Pop()
        {
            var value = memory.ReadInt16(SP);
            SP += 2;
            return value;
        }
    }
}
