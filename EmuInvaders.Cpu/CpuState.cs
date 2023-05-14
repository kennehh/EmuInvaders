using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuInvaders.Cpu
{
    public class CpuState
    {
        public Stack Stack { get; }
        public Memory Memory { get; }
        public FlagsState Flags { get; }
        public IInputHandler InputHandler { get; set; }
        public IOutputHandler OutputHandler { get; set; }
        public bool InterruptsEnabled { get; set; } = false;
        public bool Halted { get; set; } = false;
        public string CpuDiagMessage { get; set; }

        public ushort PC { get; set; }

        public byte A { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }

        public ushort BC
        {
            get => Utils.GetInt16(B, C);
            set
            {
                C = Utils.GetHighInt8(value);
                B = Utils.GetLowInt8(value);
            }
        }

        public ushort DE
        {
            get => Utils.GetInt16(D, E);
            set
            {
                E = Utils.GetHighInt8(value);
                D = Utils.GetLowInt8(value);
            }
        }

        public ushort HL
        {
            get => Utils.GetInt16(H, L);
            set
            {
                L = Utils.GetHighInt8(value);
                H = Utils.GetLowInt8(value);
            }
        }

        public byte M
        {
            get => Memory.ReadInt8(HL);
            set => Memory.WriteInt8(HL, value);
        }

        public CpuState(Memory memory)
        {
            Memory = memory;
            Stack = new Stack(memory);
            Flags = new FlagsState();
        }

        public byte GetImmediateInt8()
        {
            return Memory.ReadInt8(PC + 1);
        }

        public ushort GetImmediateInt16()
        {
            return Memory.ReadInt16(PC + 1);
        }

        public byte ReadRegister(Register register)
        {
            switch (register)
            {
                case Register.A:
                    return A;
                case Register.B:
                    return B;
                case Register.C:
                    return C;
                case Register.D:
                    return D;
                case Register.E:
                    return E;
                case Register.H:
                    return H;
                case Register.L:
                    return L;
                case Register.M:
                    return M;
                default:
                    throw new NotImplementedException();
            }
        }

        public ushort ReadRegisterPair(RegisterPair registerPair)
        {
            switch (registerPair)
            {
                case RegisterPair.BC:
                    return BC;
                case RegisterPair.DE:
                    return DE;
                case RegisterPair.HL:
                    return HL;
                case RegisterPair.SP:
                    return Stack.SP;
                default:
                    throw new NotImplementedException();
            }
        }

        public void WriteToRegister(Register register, byte value)
        {
            switch (register)
            {
                case Register.A:
                    A = value;
                    break;
                case Register.B:
                    B = value;
                    break;
                case Register.C:
                    C = value;
                    break;
                case Register.D:
                    D = value;
                    break;
                case Register.E:
                    E = value;
                    break;
                case Register.H:
                    H = value;
                    break;
                case Register.L:
                    L = value;
                    break;
                case Register.M:
                    M = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void WriteToRegisterPair(RegisterPair registerPair, ushort value)
        {
            switch (registerPair)
            {
                case RegisterPair.BC:
                    BC = value;
                    break;
                case RegisterPair.DE:
                    DE = value;
                    break;
                case RegisterPair.HL:
                    HL = value;
                    break;
                case RegisterPair.SP:
                    Stack.SP = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
