using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Cpu
{
    public class FlagsState
    {
        public bool Zero { get; internal set; }
        public bool Sign { get; internal set; }
        public bool Parity { get; internal set; }
        public bool Carry { get; internal set; }
        public bool AuxCarry { get; internal set; }

        // S Z 0 AC 0 P 1 CY
        public byte PSW
        {
            get
            {
                var psw = Carry.ToBit() << 0|
                          1 << 1 |
                          Parity.ToBit() << 2 |
                          0 << 3 |
                          AuxCarry.ToBit() << 4 |
                          0 << 5 |
                          Zero.ToBit() << 6 |
                          Sign.ToBit() << 7;
                return (byte)psw;
            }
            internal set
            {
                Carry =     (value & 0b00000001) == 0b00000001;
                Parity =    (value & 0b00000100) == 0b00000100;
                AuxCarry =  (value & 0b00010000) == 0b00010000;
                Zero =      (value & 0b01000000) == 0b01000000;
                Sign =      (value & 0b10000000) == 0b10000000;
            }
        }

        internal void SetCarry(int i)
        {
            Carry = (i >> 8) != 0;
        }

        internal void SetNonCarryFlags(int value)
        {
            var byteValue = (byte)value;
            Sign = (byteValue & 0x80) != 0;
            Zero = byteValue == 0;
            Parity = parityTable[byteValue] == 1;
        }

        internal void SetAddAuxCarry(byte b1, byte b2)
        {
            AuxCarry = ((b1 & 0xF) + (b2 & 0xF)) > 0xF;
        }

        internal void SetAddAuxCarryWithCarry(byte b1, byte b2)
        {
            AuxCarry = ((b1 & 0xF) + (b2 & 0xF)) >= 0xF;
        }

        internal void SetSubAuxCarry(byte b1, byte b2)
        {
            AuxCarry = (b2 & 0xF) <= (b1 & 0xF);
        }

        internal void SetSubAuxCarryWithCarry(byte b1, byte b2)
        {
            AuxCarry = (b2 & 0xF) < (b1 & 0xF);
        }

        private readonly static int[] parityTable = 
        {
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
            0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
            0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
            0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
            0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
            0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
            0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
            0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
            0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
            1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1
        };
    }
}
