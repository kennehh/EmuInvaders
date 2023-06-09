﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EmuInvaders.Cpu
{
    internal static class Operations
    {

        public static int MOV(CpuState state, Register rLeft, Register rRight)
        {
            var value = state.ReadRegister(rRight);
            state.WriteToRegister(rLeft, value);
            return rLeft == Register.M || rRight == Register.M ? 7 : 5;
        }

        public static int MVI(CpuState state, Register register)
        {
            state.WriteToRegister(register, state.GetImmediateInt8());
            return register == Register.M ? 10 : 7;
        }

        public static int LXI(CpuState state, RegisterPair registerPair)
        {
            state.WriteToRegisterPair(registerPair, state.GetImmediateInt16());
            return 10;
        }

        public static int LDA(CpuState state)
        {
            var address = state.GetImmediateInt16();
            state.A = state.Memory.ReadInt8(address);
            return 13;
        }

        public static int STA(CpuState state)
        {
            var address = state.GetImmediateInt16();
            state.Memory.WriteInt8(address, state.A);
            return 13;
        }

        public static int LHLD(CpuState state)
        {
            var address = state.GetImmediateInt16();
            state.HL = state.Memory.ReadInt16(address);
            return 16;
        }

        public static int SHLD(CpuState state)
        {
            var address = state.GetImmediateInt16();
            state.Memory.WriteInt16(address, state.HL);
            return 16;
        }

        public static int LDAX(CpuState state, RegisterPair registerPair)
        {
            var address = state.ReadRegisterPair(registerPair);
            state.A = state.Memory.ReadInt8(address);
            return 7;
        }

        public static int STAX(CpuState state, RegisterPair registerPair)
        {
            var address = state.ReadRegisterPair(registerPair);
            state.Memory.WriteInt8(address, state.A);
            return 7;
        }

        public static int XCHG(CpuState state)
        {
            (state.HL, state.DE) = (state.DE, state.HL);
            return 4;
        }

        public static int ADD(CpuState state, Register register)
        {
            var value = state.ReadRegister(register);
            return ADD(state, value, register == Register.M, false);
        }

        public static int ADI(CpuState state)
        {
            var value = state.GetImmediateInt8();
            return ADD(state, value, true, false);
        }

        public static int ADC(CpuState state, Register register)
        {
            var value = state.ReadRegister(register);
            return ADD(state, value, register == Register.M, true);
        }

        public static int ACI(CpuState state)
        {
            var value = state.GetImmediateInt8();
            return ADD(state, value, true, true);
        }

        public static int SUB(CpuState state, Register register)
        {
            var value = state.ReadRegister(register);
            return SUB(state, value, register == Register.M, false);
        }

        public static int SUI(CpuState state)
        {
            var value = state.GetImmediateInt8();
            return SUB(state, value, true, false);
        }

        public static int SBB(CpuState state, Register register)
        {
            var value = state.ReadRegister(register);
            return SUB(state, value, register == Register.M, true);
        }

        public static int SBI(CpuState state)
        {
            var value = state.GetImmediateInt8();
            return SUB(state, value, true, true);
        }

        public static int INR(CpuState state, Register register)
        {
            var value = state.ReadRegister(register);
            var result = value + 1;
            state.Flags.SetNonCarryFlags(result);
            state.Flags.SetAddAuxCarry(value, 1);
            state.WriteToRegister(register, (byte)result);
            return register == Register.M ? 10 : 5;
        }

        public static int DCR(CpuState state, Register register)
        {
            var value = state.ReadRegister(register);
            var result = value - 1;
            state.Flags.SetNonCarryFlags(result);
            state.Flags.SetSubAuxCarry(value, 1);
            state.WriteToRegister(register, (byte)result);
            return register == Register.M ? 10 : 5;
        }

        public static int INX(CpuState state, RegisterPair registerPair)
        {
            var result = state.ReadRegisterPair(registerPair) + 1;
            state.WriteToRegisterPair(registerPair, (ushort)result);
            return 5;
        }

        public static int DCX(CpuState state, RegisterPair registerPair)
        {
            var result = state.ReadRegisterPair(registerPair) - 1;
            state.WriteToRegisterPair(registerPair, (ushort)result);
            return 5;
        }

        public static int DAD(CpuState state, RegisterPair registerPair)
        {
            var result = state.HL + state.ReadRegisterPair(registerPair);
            state.Flags.Carry = result > 0xffff;
            state.HL = (ushort)result;
            return 10;
        }

        public static int DAA(CpuState state)
        {
            byte add = 0;

            if (state.Flags.AuxCarry || (state.A & 0x0f) > 9)
            {
                add = 0x06;
            }

            if (state.Flags.Carry || (state.A >> 4) > 9 || (state.A >> 4) >= 9 && (state.A & 0x0f) > 9)
            {
                add |= 0x60;
                state.Flags.Carry = true;
            }

            var result = state.A + add;
            state.Flags.SetAddAuxCarry(state.A, add);
            state.Flags.SetNonCarryFlags(result);
            state.A = (byte)result;

            return 4;
        }

        private static int ADD(CpuState state, byte value, bool isMemoryOperation, bool withCarry)
        {
            var result = state.A + value;
            if (withCarry && state.Flags.Carry)
            {
                result += 1;
                state.Flags.SetAddAuxCarryWithCarry(state.A, value);
            }
            else
            {
                state.Flags.SetAddAuxCarry(state.A, value);
            }

            state.Flags.SetNonCarryFlags(result);
            state.Flags.SetCarry(result);
            
            state.A = (byte)result;
            return isMemoryOperation ? 7 : 4;
        }

        private static int SUB(CpuState state, byte value, bool isMemoryOperation, bool withBorrow)
        {
            var result = state.A - value;
            if (withBorrow && state.Flags.Carry)
            {
                result -= 1;
                state.Flags.SetSubAuxCarryWithCarry(state.A, value);
            }
            else
            {
                state.Flags.SetSubAuxCarry(state.A, value);
            }

            state.Flags.SetNonCarryFlags(result);
            state.Flags.SetCarry(result);

            state.A = (byte)result;
            return isMemoryOperation ? 7 : 4;
        }

        public static int ANA(CpuState state, Register register)
        {
            var value = state.ReadRegister(register);
            var result = state.A & value;

            state.Flags.SetNonCarryFlags(result);
            state.Flags.AuxCarry = ((state.A | value) & 0x08) != 0; // special case on 8080 documented by intel p1-12
            state.Flags.Carry = false;
            state.A = (byte)result;
            return register == Register.M ? 7 : 4;
        }

        public static int ANI(CpuState state)
        {
            var value = state.GetImmediateInt8();
            var result = state.A & value;

            state.Flags.SetNonCarryFlags(result);
            state.Flags.AuxCarry = ((state.A | value) & 0x08) != 0; // special case on 8080 documented by intel p1-12
            state.Flags.Carry = false;
            state.A = (byte)result;
            return 7;
        }

        public static int XRA(CpuState state, Register register)
        {
            var result = state.A ^ state.ReadRegister(register);
            state.Flags.AuxCarry = false;
            state.Flags.Carry = false;
            state.Flags.SetNonCarryFlags(result);
            state.A = (byte)result;
            return register == Register.M ? 7 : 4;
        }

        public static int XRI(CpuState state)
        {
            var result = state.A ^ state.GetImmediateInt8();
            state.Flags.AuxCarry = false;
            state.Flags.Carry = false;
            state.Flags.SetNonCarryFlags(result);
            state.A = (byte)result;
            return 7;
        }

        public static int ORA(CpuState state, Register register)
        {
            var result = state.A | state.ReadRegister(register);
            state.Flags.AuxCarry = false;
            state.Flags.Carry = false;
            state.Flags.SetNonCarryFlags(result);
            state.A = (byte)result;
            return register == Register.M ? 7 : 4;
        }

        public static int ORI(CpuState state)
        {
            var result = state.A | state.GetImmediateInt8();
            state.Flags.AuxCarry = false;
            state.Flags.Carry = false;
            state.Flags.SetNonCarryFlags(result);
            state.A = (byte)result;
            return 7;
        }

        public static int CMP(CpuState state, Register register)
        {
            var value = state.ReadRegister(register);
            var result = state.A - value;
            state.Flags.SetSubAuxCarry(state.A, value);
            state.Flags.SetNonCarryFlags(result);
            state.Flags.SetCarry(result);
            return register == Register.M ? 7 : 4;
        }

        public static int CPI(CpuState state)
        {
            var value = state.GetImmediateInt8();
            var result = state.A - value;
            state.Flags.SetSubAuxCarry(state.A, value);
            state.Flags.SetNonCarryFlags(result);
            state.Flags.SetCarry(result);
            return 7;
        }

        public static int RLC(CpuState state)
        {
            var result = (state.A << 1) | ((state.A & 0x80) >> 7);
            state.Flags.Carry = (state.A & 0x80) == 0x80;
            state.A = (byte)result;
            return 4;
        }

        public static int RRC(CpuState state)
        {
            var result = ((state.A & 1) << 7) | (state.A >> 1);
            state.Flags.Carry = (state.A & 1) == 1;
            state.A = (byte)result;
            return 4;
        }

        public static int RAL(CpuState state)
        {
            var cy = state.Flags.Carry.ToBit();
            var result = (state.A << 1) | cy;
            state.Flags.Carry = (state.A & 0x80) == 0x80;
            state.A = (byte)result;
            return 4;
        }

        public static int RAR(CpuState state)
        {
            var cy = state.Flags.Carry.ToBit();
            var result = (cy << 7) | (state.A >> 1);
            state.Flags.Carry = (state.A & 1) == 1;
            state.A = (byte)result;
            return 4;
        }

        public static int CMA(CpuState state)
        {
            state.A = (byte)~state.A;
            return 4;
        }

        public static int CMC(CpuState state)
        {
            state.Flags.Carry = !state.Flags.Carry;
            return 4;
        }

        public static int STC(CpuState state)
        {
            state.Flags.Carry = true;
            return 4;
        }

        public static int JMP(CpuState state, bool condition = true)
        {
            var pc = state.GetImmediateInt16();
            if (condition)
            {
                state.PC = pc;
            }
            return 10;
        }

        public static int CALL(CpuState state, bool condition = true)
        {
            var pc = state.GetImmediateInt16();

            if (condition)
            {
                state.Stack.Push((ushort)(state.PC + 3));
                state.PC = pc;
                return 17;
            }
            else
            {
                return 11;
            }
        }

        public static int RET(CpuState state)
        {
            state.PC = state.Stack.Pop();
            return 10;
        }

        public static int RET(CpuState state, bool condition)
        {
            if (condition)
            {
                state.PC = state.Stack.Pop();
                return 11;
            }
            else
            {
                return 5;
            }
        }

        public static int RST(CpuState state, int rstNumber)
        {
            state.Stack.Push(state.PC);
            state.PC = (ushort)rstNumber;
            return 11;
        }

        public static int PCHL(CpuState state)
        {
            state.PC = state.HL;
            return 5;
        }

        public static int PUSH(CpuState cpuState, RegisterPair registerPair)
        {
            cpuState.Stack.Push(cpuState.ReadRegisterPair(registerPair));
            return 11;
        }

        public static int PUSH_PSW(CpuState cpuState)
        {
            cpuState.Stack.Push(cpuState.Flags.PSW, cpuState.A);
            return 11;
        }

        public static int POP(CpuState cpuState, RegisterPair registerPair)
        {
            cpuState.WriteToRegisterPair(registerPair, cpuState.Stack.Pop());
            return 10;
        }

        public static int POP_PSW(CpuState cpuState)
        {
            var value = cpuState.Stack.Pop();
            cpuState.A = Utils.GetMostSignificantByte(value);
            cpuState.Flags.PSW = Utils.GetLeastSignificantByte(value);
            return 10;
        }

        public static int XTHL(CpuState cpuState)
        {
            var newHL = cpuState.Memory.ReadInt16(cpuState.Stack.SP);
            cpuState.Memory.WriteInt16(cpuState.Stack.SP, cpuState.HL);
            cpuState.HL = newHL;
            return 18;
        }

        public static int SPHL(CpuState cpuState)
        {
            cpuState.Stack.SP = cpuState.HL;
            return 5;
        }

        public static int IN(CpuState cpuState)
        {
            var port = cpuState.GetImmediateInt8();
            if (cpuState.InputDevices.TryGetValue(port, out var inputDevice))
            {
                cpuState.A = inputDevice();
            }
            else
            {
                cpuState.A = 0;
            }
            return 10;
        }

        public static int OUT(CpuState cpuState)
        {
            var port = cpuState.GetImmediateInt8();
            if (cpuState.OutputDevices.TryGetValue(port, out var outputDevice))
            {
                outputDevice(cpuState.A);
            }
            return 10;
        }

        public static int EI(CpuState cpuState)
        {
            cpuState.InterruptsEnabled = true;
            return 4;
        }

        public static int DI(CpuState cpuState)
        {
            cpuState.InterruptsEnabled = false;
            return 4;
        }

        public static int HLT(CpuState cpuState)
        {
            cpuState.Halted = true;
            return 7;
        }

        public static int NOP(CpuState cpuState) => 4;
    }
}
