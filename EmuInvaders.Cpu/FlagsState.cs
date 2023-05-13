using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Cpu
{
    public class FlagsState
    {
        public bool Zero { get; set; }
        public bool SignMinus { get; set; }
        public bool ParityEven { get; set; }
        public bool Carry { get; set; }
        public bool AuxCarry { get; set; }

        public byte PSW
        {
            get
            {
                var psw = Zero.ToBit() |
                          SignMinus.ToBit() << 1 |
                          ParityEven.ToBit() << 2 |
                          Carry.ToBit() << 3 |
                          AuxCarry.ToBit() << 4;
                return (byte)psw;
            }
            set
            {
                Zero = (value & 0x01) == 0x01;
                SignMinus = (value & 0x02) == 0x02;
                ParityEven = (value & 0x04) == 0x04;
                Carry = (value & 0x08) == 0x08;
                AuxCarry = (value & 0x10) == 0x10;
            }
        }

        public void SetFlags(byte originalValue, int result, FlagOptions options)
        {
            var byteResult = (byte)result;

            if (options.HasFlag(FlagOptions.Zero))
            {
                Zero = byteResult == 0;
            }
            if (options.HasFlag(FlagOptions.Sign))
            {
                SignMinus = (result & 0x80) != 0;
            }
            if (options.HasFlag(FlagOptions.Parity))
            {
                ParityEven = CalculateParity(byteResult);
            }

            if (options.HasFlag(FlagOptions.CarryClear))
            {
                Carry = false;
            }
            else if (options.HasFlag(FlagOptions.Carry))
            {
                Carry = result > 0xff || result < 0;
            }

            if (options.HasFlag(FlagOptions.AuxCarryClear))
            {
                AuxCarry = false;
            }
            else if (options.HasFlag(FlagOptions.AuxCarry))
            {
                AuxCarry = (originalValue & 0xf) > (result & 0xf);
            }
        }

        private static bool CalculateParity(byte value)
        {
            var setBits = 0;
            for (var i = 0; i < 8; i++)
            {
                if ((value & 0x01) == 1)
                {
                    setBits++;
                }
                value >>= 1;
            }
            return setBits % 2 == 0;
        }
    }
}
