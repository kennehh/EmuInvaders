using System;

namespace EmuInvaders.Cpu
{
    public enum Register
    {
        A, B, C, D, E, H, L, M
    }

    public enum RegisterPair
    {
        BC, DE, HL, SP
    }

    [Flags]
    public enum FlagOptions
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
