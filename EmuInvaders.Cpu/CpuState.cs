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

        public bool InterruptsEnabled { get; internal set; } = false;
        public bool Halted { get; internal set; } = false;
        public string CpuDiagMessage { get; internal set; }

        public ushort PC { get; internal set; }

        public byte A { get; internal set; }
        public byte B { get; internal set; }
        public byte C { get; internal set; }
        public byte D { get; internal set; }
        public byte E { get; internal set; }
        public byte H { get; internal set; }
        public byte L { get; internal set; }

        public ushort BC
        {
            get => Utils.GetInt16(B, C);
            internal set
            {
                C = Utils.GetHighInt8(value);
                B = Utils.GetLowInt8(value);
            }
        }

        public ushort DE
        {
            get => Utils.GetInt16(D, E);
            internal set
            {
                E = Utils.GetHighInt8(value);
                D = Utils.GetLowInt8(value);
            }
        }

        public ushort HL
        {
            get => Utils.GetInt16(H, L);
            internal set
            {
                L = Utils.GetHighInt8(value);
                H = Utils.GetLowInt8(value);
            }
        }

        public byte M
        {
            get => Memory.ReadInt8(HL);
            internal set => Memory.WriteInt8(HL, value);
        }

        internal readonly Dictionary<byte, Func<byte>> InputDevices = new Dictionary<byte, Func<byte>>();
        internal readonly Dictionary<byte, Action<byte>> OutputDevices = new Dictionary<byte, Action<byte>>();


        internal CpuState(Memory memory)
        {
            Memory = memory;
            Stack = new Stack(memory);
            Flags = new FlagsState();
        }

        internal byte GetImmediateInt8()
        {
            return Memory.ReadInt8(PC + 1);
        }

        internal ushort GetImmediateInt16()
        {
            return Memory.ReadInt16(PC + 1);
        }

        internal byte ReadRegister(Register register)
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

        internal ushort ReadRegisterPair(RegisterPair registerPair)
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

        internal void WriteToRegister(Register register, byte value)
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

        internal void WriteToRegisterPair(RegisterPair registerPair, ushort value)
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
