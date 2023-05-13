using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Cpu
{
    internal static class Utils
    {
        public static ushort GetInt16(byte low, byte high) => (ushort)(low << 8 | high);
        public static byte GetHighInt8(ushort value) => (byte)(value & 0xff); 
        public static byte GetLowInt8(ushort value) => (byte)((value >> 8) & 0xff);
        public static byte ToBit(this bool value) => (byte)(value ? 1 : 0);
    }
}
