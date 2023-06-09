﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EmuInvaders.Cpu
{
    public class Memory
    {
        public ReadOnlyMemory<byte> FrameBuffer { get; private set; }

        public int Length => memory.Length;

        private byte[] memory;
        private ushort readOnlyStart = 0;
        private ushort readOnlyEnd = 0;

        internal Memory(int size)
        {
            memory = new byte[size];
        }

        public void SetFrameBufferRegion(ushort start, ushort end)
        {
            FrameBuffer = GetSubsetOfMemory(start, end);
        }

        internal void Load(byte[] data, ushort dstOffset)
        {
            Buffer.BlockCopy(data, 0, memory, dstOffset, data.Length);
        }

        internal void Load(byte[] data, ushort srcOffset, ushort dstOffset, ushort length)
        {
            Buffer.BlockCopy(data, srcOffset, memory, dstOffset, length);
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
            memory[address] = Utils.GetLeastSignificantByte(value);
            memory[address + 1] = Utils.GetMostSignificantByte(value);
        }

        public ReadOnlyMemory<byte> GetSubsetOfMemory(int start, int end)
        {
            return new ReadOnlyMemory<byte>(memory, start, end);
        }
    }
}
