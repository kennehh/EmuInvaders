using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Cpu
{
    internal static class Utils
    {
        public static ushort GetInt16(byte msb, byte lsb) => (ushort)(msb << 8 | lsb);
        public static byte GetLeastSignificantByte(ushort value) => (byte)(value & 0xff); 
        public static byte GetMostSignificantByte(ushort value) => (byte)((value >> 8) & 0xff);
        public static byte ToBit(this bool value) => (byte)(value ? 1 : 0);
    }
}
