using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EmuInvaders.Cpu
{
    public class Memory
    {
        public ReadOnlyMemory<byte> VideoBuffer { get; private set; }

        public int Length => memory.Length;

        private byte[] memory;
        private ushort readOnlyStart = 0;
        private ushort readOnlyEnd = 0;

        public Memory(int size)
        {
            memory = new byte[size];
        }

        public void SetVideoBuffer(ushort start, ushort end)
        {
            VideoBuffer = GetSubsetOfMemory(start, end);
        }

        internal void Load(byte[] data, ushort dstOffset)
        {
            Buffer.BlockCopy(data, 0, memory, dstOffset, data.Length);
        }

        internal void Load(byte[] data, ushort srcOffset, ushort dstOffset, ushort length)
        {
            Buffer.BlockCopy(data, srcOffset, memory, dstOffset, length);
        }

        internal void SetReadOnly(ushort start, ushort end)
        {
            readOnlyStart = start;
            readOnlyEnd = end;
        }

        internal byte ReadInt8(int address)
        {
            return memory[address];
        }

        internal ushort ReadInt16(int address)
        {
            var high = memory[address];
            var low = memory[address + 1];
            return Utils.GetInt16(low, high);
        }

        internal void WriteInt8(int address, byte value)
        {
            memory[address] = value;
        }

        internal void WriteInt16(int address, ushort value)
        {
            memory[address] = Utils.GetHighInt8(value);
            memory[address + 1] = Utils.GetLowInt8(value);
        }

        public ReadOnlyMemory<byte> GetSubsetOfMemory(int start, int end)
        {
            return new ReadOnlyMemory<byte>(memory, start, end);
        }

        private void CheckIfReadOnly(int address)
        {
            if (address >= readOnlyStart && address <= readOnlyEnd)
            {
                throw new InvalidOperationException("Cannot write at this address - it is set to read only");
            }
        }
    }
}
