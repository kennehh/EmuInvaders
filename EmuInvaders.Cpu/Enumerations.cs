using System;

namespace EmuInvaders.Cpu
{
    internal enum Register
    {
        A, B, C, D, E, H, L, M
    }

    internal enum RegisterPair
    {
        BC, DE, HL, SP
    }

    [Flags]
    internal enum FlagOptions
    {
        Zero = 0,
        Sign = 1,
        Parity = 2,
        Carry = 4,
        AuxCarry = 8,
        CarryClear = 16,
        AuxCarryClear = 32
    }
}
