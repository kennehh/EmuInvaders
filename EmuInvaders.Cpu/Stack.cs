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

        internal Stack(Memory memory)
        {
            this.memory = memory;
        }

        internal void Push(ushort value)
        {
            memory.WriteInt16((ushort)(SP - 2), value);
            SP -= 2;
        }

        internal void Push(byte lsb, byte msb)
        {
            memory.WriteInt8((ushort)(SP - 2), lsb);
            memory.WriteInt8((ushort)(SP - 1), msb);
            SP -= 2;
        }

        internal ushort Pop()
        {
            var value = memory.ReadInt16(SP);
            SP += 2;
            return value;
        }
    }
}
