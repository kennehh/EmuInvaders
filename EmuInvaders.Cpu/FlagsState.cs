using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Cpu
{
    public class FlagsState
    {
        public bool Zero { get; set; }
        public bool Sign { get; set; }
        public bool Parity { get; set; }
        public bool Carry { get; set; }
        public bool AuxCarry { get; set; }

        // S Z 0 AC 0 P 1 CY
        public byte PSW
        {
            get
            {
                //var psw = Carry.ToBit() |
                //          Parity.ToBit() << 2 |
                //          AuxCarry.ToBit() << 4 |
                //          Zero.ToBit() << 6 |
                //          Sign.ToBit() << 7;
                var psw = Carry.ToBit() |
                          1 << 1 |
                          Parity.ToBit() << 2 |
                          0 << 3 |
                          AuxCarry.ToBit() << 4 |
                          0 << 5 |
                          Zero.ToBit() << 6 |
                          Sign.ToBit() << 7;
                return (byte)psw;
            }
            set
            {
                //Carry =  (value & 0b00000001) == 1;
                //Parity = (value & 0b00000100) == 1;
                //AuxCarry = (value & 0b00010000) == 1;
                //Zero = (value & 0b01000000) == 1;
                //Sign = (value & 0b10000000) == 1;

                Carry =     (value & 0b00000001) == 0b00000001;
                Parity =    (value & 0b00000100) == 0b00000100;
                AuxCarry =  (value & 0b00010000) == 0b00010000;
                Zero =      (value & 0b01000000) == 0b01000000;
                Sign =      (value & 0b10000000) == 0b10000000;
            }
        }

        public void SetCarry(int i)
        {
            Carry = (i >> 8) != 0;
        }

        public void SetNonCarryFlags(int value)
        {
            var byteValue = (byte)value;
            Sign = (byteValue & 0x80) != 0;
            Zero = byteValue == 0;
            Parity = parityTable[byteValue] == 1;
        }

        public void SetAddAuxCarry(byte b1, byte b2)
        {
            AuxCarry = ((b1 & 0xF) + (b2 & 0xF)) > 0xF;
        }

        public void SetAddAuxCarryWithCarry(byte b1, byte b2)
        {
            AuxCarry = ((b1 & 0xF) + (b2 & 0xF)) >= 0xF;
        }

        public void SetSubAuxCarry(byte b1, byte b2)
        {
            AuxCarry = (b2 & 0xF) <= (b1 & 0xF);
        }

        public void SetSubAuxCarryWithCarry(byte b1, byte b2)
        {
            AuxCarry = (b2 & 0xF) < (b1 & 0xF);
        }

        //public void SetFlags(byte originalValue, int result, FlagOptions options)
        //{
        //    var byteResult = (byte)result;

        //    if (options.HasFlag(FlagOptions.Zero))
        //    {
        //        Zero = byteResult == 0;
        //    }
        //    if (options.HasFlag(FlagOptions.Sign))
        //    {
        //        SignMinus = (result & 0x80) == 0x80;
        //    }
        //    if (options.HasFlag(FlagOptions.Parity))
        //    {
        //        ParityEven = parityTable[byteResult] == 1;
        //    }

        //    if (options.HasFlag(FlagOptions.CarryClear))
        //    {
        //        Carry = false;
        //    }
        //    else if (options.HasFlag(FlagOptions.Carry))
        //    {
        //        Carry = result > 0xff || result < 0;
        //    }

        //    if (options.HasFlag(FlagOptions.AuxCarryClear))
        //    {
        //        AuxCarry = false;
        //    }
        //    else if (options.HasFlag(FlagOptions.AuxCarry))
        //    {
        //        AuxCarry = result > 0x09;// (originalValue & 0xf) > (result & 0xf);
        //    }
        //}

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
