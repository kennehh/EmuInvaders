using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmuInvaders.Cpu
{
    public class Intel8080
    {
        public CpuState State { get; }

        private static readonly Dictionary<byte, Opcode> opcodes = new Dictionary<byte, Opcode>
        {
            [0x00] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0x01] = new Opcode("LXI B",     3,   state => Operations.LXI(state, RegisterPair.BC)),
            [0x02] = new Opcode("STAX B",    1,   state => Operations.STAX(state, RegisterPair.BC)),
            [0x03] = new Opcode("INX B",     1,   state => Operations.INX(state, RegisterPair.BC)),
            [0x04] = new Opcode("INR B",     1,   state => Operations.INR(state, Register.B)),
            [0x05] = new Opcode("DCR B",     1,   state => Operations.DCR(state, Register.B)),
            [0x06] = new Opcode("MVI B",     2,   state => Operations.MVI(state, Register.B)),
            [0x07] = new Opcode("RLC",       1,   state => Operations.RLC(state)),
            [0x08] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0x09] = new Opcode("DAD B",     1,   state => Operations.DAD(state, RegisterPair.BC)),
            [0x0a] = new Opcode("LDAX B",    1,   state => Operations.LDAX(state, RegisterPair.BC)),
            [0x0b] = new Opcode("DCX B",     1,   state => Operations.DCX(state, RegisterPair.BC)),
            [0x0c] = new Opcode("INR C",     1,   state => Operations.INR(state, Register.C)),
            [0x0d] = new Opcode("DCR C",     1,   state => Operations.DCR(state, Register.C)),
            [0x0e] = new Opcode("MVI C",     2,   state => Operations.MVI(state, Register.C)),
            [0x0f] = new Opcode("RRC",       1,   state => Operations.RRC(state)),
            [0x10] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0x11] = new Opcode("LXI D",     3,   state => Operations.LXI(state, RegisterPair.DE)),
            [0x12] = new Opcode("STAX D",    1,   state => Operations.STAX(state, RegisterPair.DE)),
            [0x13] = new Opcode("INX D",     1,   state => Operations.INX(state, RegisterPair.DE)),
            [0x14] = new Opcode("INR D",     1,   state => Operations.INR(state, Register.D)),
            [0x15] = new Opcode("DCR D",     1,   state => Operations.DCR(state, Register.D)),
            [0x16] = new Opcode("MVI D",     2,   state => Operations.MVI(state, Register.D)),
            [0x17] = new Opcode("RAL",       1,   state => Operations.RAL(state)),
            [0x18] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0x19] = new Opcode("DAD D",     1,   state => Operations.DAD(state, RegisterPair.DE)),
            [0x1a] = new Opcode("LDAX D",    1,   state => Operations.LDAX(state, RegisterPair.DE)),
            [0x1b] = new Opcode("DCX D",     1,   state => Operations.DCX(state, RegisterPair.DE)),
            [0x1c] = new Opcode("INR E",     1,   state => Operations.INR(state, Register.E)),
            [0x1d] = new Opcode("DCR E",     1,   state => Operations.DCR(state, Register.E)),
            [0x1e] = new Opcode("MVI E",     2,   state => Operations.MVI(state, Register.E)),
            [0x1f] = new Opcode("RAR",       1,   state => Operations.RAR(state)),
            [0x20] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0x21] = new Opcode("LXI H",     3,   state => Operations.LXI(state, RegisterPair.HL)),
            [0x22] = new Opcode("SHLD adr",  3,   state => Operations.SHLD(state)),
            [0x23] = new Opcode("INX H",     1,   state => Operations.INX(state, RegisterPair.HL)),
            [0x24] = new Opcode("INR H",     1,   state => Operations.INR(state, Register.H)),
            [0x25] = new Opcode("DCR H",     1,   state => Operations.DCR(state, Register.H)),
            [0x26] = new Opcode("MVI H",     2,   state => Operations.MVI(state, Register.H)),
            [0x27] = new Opcode("DAA",       1,   state => Operations.DAA(state)),
            [0x28] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0x29] = new Opcode("DAD H",     1,   state => Operations.DAD(state, RegisterPair.HL)),
            [0x2a] = new Opcode("LHLD adr",  3,   state => Operations.LHLD(state)),
            [0x2b] = new Opcode("DCX H",     1,   state => Operations.DCX(state, RegisterPair.HL)),
            [0x2c] = new Opcode("INR L",     1,   state => Operations.INR(state, Register.L)),
            [0x2d] = new Opcode("DCR L",     1,   state => Operations.DCR(state, Register.L)),
            [0x2e] = new Opcode("MVI L",     2,   state => Operations.MVI(state, Register.L)),
            [0x2f] = new Opcode("CMA",       1,   state => Operations.CMA(state)),
            [0x30] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0x31] = new Opcode("LXI SP",    3,   state => Operations.LXI(state, RegisterPair.SP)),
            [0x32] = new Opcode("STA adr",   3,   state => Operations.STA(state)),
            [0x33] = new Opcode("INX SP",    1,   state => Operations.INX(state, RegisterPair.SP)),
            [0x34] = new Opcode("INR M",     1,   state => Operations.INR(state, Register.M)),
            [0x35] = new Opcode("DCR M",     1,   state => Operations.DCR(state, Register.M)),
            [0x36] = new Opcode("MVI M",     2,   state => Operations.MVI(state, Register.M)),
            [0x37] = new Opcode("STC",       1,   state => Operations.STC(state)),
            [0x38] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0x39] = new Opcode("DAD SP",    1,   state => Operations.DAD(state, RegisterPair.SP)),
            [0x3a] = new Opcode("LDA adr",   3,   state => Operations.LDA(state)),
            [0x3b] = new Opcode("DCX SP",    1,   state => Operations.DCX(state, RegisterPair.SP)),
            [0x3c] = new Opcode("INR A",     1,   state => Operations.INR(state, Register.A)),
            [0x3d] = new Opcode("DCR A",     1,   state => Operations.DCR(state, Register.A)),
            [0x3e] = new Opcode("MVI A",     2,   state => Operations.MVI(state, Register.A)),
            [0x3f] = new Opcode("CMC",       1,   state => Operations.CMC(state)),
            [0x40] = new Opcode("MOV B,B",   1,   state => Operations.MOV(state, Register.B, Register.B)),
            [0x41] = new Opcode("MOV B,C",   1,   state => Operations.MOV(state, Register.B, Register.C)),
            [0x42] = new Opcode("MOV B,D",   1,   state => Operations.MOV(state, Register.B, Register.D)),
            [0x43] = new Opcode("MOV B,E",   1,   state => Operations.MOV(state, Register.B, Register.E)),
            [0x44] = new Opcode("MOV B,H",   1,   state => Operations.MOV(state, Register.B, Register.H)),
            [0x45] = new Opcode("MOV B,L",   1,   state => Operations.MOV(state, Register.B, Register.L)),
            [0x46] = new Opcode("MOV B,M",   1,   state => Operations.MOV(state, Register.B, Register.M)),
            [0x47] = new Opcode("MOV B,A",   1,   state => Operations.MOV(state, Register.B, Register.A)),
            [0x48] = new Opcode("MOV C,B",   1,   state => Operations.MOV(state, Register.C, Register.B)),
            [0x49] = new Opcode("MOV C,C",   1,   state => Operations.MOV(state, Register.C, Register.C)),
            [0x4a] = new Opcode("MOV C,D",   1,   state => Operations.MOV(state, Register.C, Register.D)),
            [0x4b] = new Opcode("MOV C,E",   1,   state => Operations.MOV(state, Register.C, Register.E)),
            [0x4c] = new Opcode("MOV C,H",   1,   state => Operations.MOV(state, Register.C, Register.H)),
            [0x4d] = new Opcode("MOV C,L",   1,   state => Operations.MOV(state, Register.C, Register.L)),
            [0x4e] = new Opcode("MOV C,M",   1,   state => Operations.MOV(state, Register.C, Register.M)),
            [0x4f] = new Opcode("MOV C,A",   1,   state => Operations.MOV(state, Register.C, Register.A)),
            [0x50] = new Opcode("MOV D,B",   1,   state => Operations.MOV(state, Register.D, Register.B)),
            [0x51] = new Opcode("MOV D,C",   1,   state => Operations.MOV(state, Register.D, Register.C)),
            [0x52] = new Opcode("MOV D,D",   1,   state => Operations.MOV(state, Register.D, Register.D)),
            [0x53] = new Opcode("MOV D,E",   1,   state => Operations.MOV(state, Register.D, Register.E)),
            [0x54] = new Opcode("MOV D,H",   1,   state => Operations.MOV(state, Register.D, Register.H)),
            [0x55] = new Opcode("MOV D,L",   1,   state => Operations.MOV(state, Register.D, Register.L)),
            [0x56] = new Opcode("MOV D,M",   1,   state => Operations.MOV(state, Register.D, Register.M)),
            [0x57] = new Opcode("MOV D,A",   1,   state => Operations.MOV(state, Register.D, Register.A)),
            [0x58] = new Opcode("MOV E,B",   1,   state => Operations.MOV(state, Register.E, Register.B)),
            [0x59] = new Opcode("MOV E,C",   1,   state => Operations.MOV(state, Register.E, Register.C)),
            [0x5a] = new Opcode("MOV E,D",   1,   state => Operations.MOV(state, Register.E, Register.D)),
            [0x5b] = new Opcode("MOV E,E",   1,   state => Operations.MOV(state, Register.E, Register.E)),
            [0x5c] = new Opcode("MOV E,H",   1,   state => Operations.MOV(state, Register.E, Register.H)),
            [0x5d] = new Opcode("MOV E,L",   1,   state => Operations.MOV(state, Register.E, Register.L)),
            [0x5e] = new Opcode("MOV E,M",   1,   state => Operations.MOV(state, Register.E, Register.M)),
            [0x5f] = new Opcode("MOV E,A",   1,   state => Operations.MOV(state, Register.E, Register.A)),
            [0x60] = new Opcode("MOV H,B",   1,   state => Operations.MOV(state, Register.H, Register.B)),
            [0x61] = new Opcode("MOV H,C",   1,   state => Operations.MOV(state, Register.H, Register.C)),
            [0x62] = new Opcode("MOV H,D",   1,   state => Operations.MOV(state, Register.H, Register.D)),
            [0x63] = new Opcode("MOV H,E",   1,   state => Operations.MOV(state, Register.H, Register.E)),
            [0x64] = new Opcode("MOV H,H",   1,   state => Operations.MOV(state, Register.H, Register.H)),
            [0x65] = new Opcode("MOV H,L",   1,   state => Operations.MOV(state, Register.H, Register.L)),
            [0x66] = new Opcode("MOV H,M",   1,   state => Operations.MOV(state, Register.H, Register.M)),
            [0x67] = new Opcode("MOV H,A",   1,   state => Operations.MOV(state, Register.H, Register.A)),
            [0x68] = new Opcode("MOV L,B",   1,   state => Operations.MOV(state, Register.L, Register.B)),
            [0x69] = new Opcode("MOV L,C",   1,   state => Operations.MOV(state, Register.L, Register.C)),
            [0x6a] = new Opcode("MOV L,D",   1,   state => Operations.MOV(state, Register.L, Register.D)),
            [0x6b] = new Opcode("MOV L,E",   1,   state => Operations.MOV(state, Register.L, Register.E)),
            [0x6c] = new Opcode("MOV L,H",   1,   state => Operations.MOV(state, Register.L, Register.H)),
            [0x6d] = new Opcode("MOV L,L",   1,   state => Operations.MOV(state, Register.L, Register.L)),
            [0x6e] = new Opcode("MOV L,M",   1,   state => Operations.MOV(state, Register.L, Register.M)),
            [0x6f] = new Opcode("MOV L,A",   1,   state => Operations.MOV(state, Register.L, Register.A)),
            [0x70] = new Opcode("MOV M,B",   1,   state => Operations.MOV(state, Register.M, Register.B)),
            [0x71] = new Opcode("MOV M,C",   1,   state => Operations.MOV(state, Register.M, Register.C)),
            [0x72] = new Opcode("MOV M,D",   1,   state => Operations.MOV(state, Register.M, Register.D)),
            [0x73] = new Opcode("MOV M,E",   1,   state => Operations.MOV(state, Register.M, Register.E)),
            [0x74] = new Opcode("MOV M,H",   1,   state => Operations.MOV(state, Register.M, Register.H)),
            [0x75] = new Opcode("MOV M,L",   1,   state => Operations.MOV(state, Register.M, Register.L)),
            [0x76] = new Opcode("HLT",       1,   state => Operations.HLT(state)),
            [0x77] = new Opcode("MOV M,A",   1,   state => Operations.MOV(state, Register.M, Register.A)),
            [0x78] = new Opcode("MOV A,B",   1,   state => Operations.MOV(state, Register.A, Register.B)),
            [0x79] = new Opcode("MOV A,C",   1,   state => Operations.MOV(state, Register.A, Register.C)),
            [0x7a] = new Opcode("MOV A,D",   1,   state => Operations.MOV(state, Register.A, Register.D)),
            [0x7b] = new Opcode("MOV A,E",   1,   state => Operations.MOV(state, Register.A, Register.E)),
            [0x7c] = new Opcode("MOV A,H",   1,   state => Operations.MOV(state, Register.A, Register.H)),
            [0x7d] = new Opcode("MOV A,L",   1,   state => Operations.MOV(state, Register.A, Register.L)),
            [0x7e] = new Opcode("MOV A,M",   1,   state => Operations.MOV(state, Register.A, Register.M)),
            [0x7f] = new Opcode("MOV A,A",   1,   state => Operations.MOV(state, Register.A, Register.A)),
            [0x80] = new Opcode("ADD B",     1,   state => Operations.ADD(state, Register.B)),
            [0x81] = new Opcode("ADD C",     1,   state => Operations.ADD(state, Register.C)),
            [0x82] = new Opcode("ADD D",     1,   state => Operations.ADD(state, Register.D)),
            [0x83] = new Opcode("ADD E",     1,   state => Operations.ADD(state, Register.E)),
            [0x84] = new Opcode("ADD H",     1,   state => Operations.ADD(state, Register.H)),
            [0x85] = new Opcode("ADD L",     1,   state => Operations.ADD(state, Register.L)),
            [0x86] = new Opcode("ADD M",     1,   state => Operations.ADD(state, Register.M)),
            [0x87] = new Opcode("ADD A",     1,   state => Operations.ADD(state, Register.A)),
            [0x88] = new Opcode("ADC B",     1,   state => Operations.ADC(state, Register.B)),
            [0x89] = new Opcode("ADC C",     1,   state => Operations.ADC(state, Register.C)),
            [0x8a] = new Opcode("ADC D",     1,   state => Operations.ADC(state, Register.D)),
            [0x8b] = new Opcode("ADC E",     1,   state => Operations.ADC(state, Register.E)),
            [0x8c] = new Opcode("ADC H",     1,   state => Operations.ADC(state, Register.H)),
            [0x8d] = new Opcode("ADC L",     1,   state => Operations.ADC(state, Register.L)),
            [0x8e] = new Opcode("ADC M",     1,   state => Operations.ADC(state, Register.M)),
            [0x8f] = new Opcode("ADC A",     1,   state => Operations.ADC(state, Register.A)),
            [0x90] = new Opcode("SUB B",     1,   state => Operations.SUB(state, Register.B)),
            [0x91] = new Opcode("SUB C",     1,   state => Operations.SUB(state, Register.C)),
            [0x92] = new Opcode("SUB D",     1,   state => Operations.SUB(state, Register.D)),
            [0x93] = new Opcode("SUB E",     1,   state => Operations.SUB(state, Register.E)),
            [0x94] = new Opcode("SUB H",     1,   state => Operations.SUB(state, Register.H)),
            [0x95] = new Opcode("SUB L",     1,   state => Operations.SUB(state, Register.L)),
            [0x96] = new Opcode("SUB M",     1,   state => Operations.SUB(state, Register.M)),
            [0x97] = new Opcode("SUB A",     1,   state => Operations.SUB(state, Register.A)),
            [0x98] = new Opcode("SBB B",     1,   state => Operations.SBB(state, Register.B)),
            [0x99] = new Opcode("SBB C",     1,   state => Operations.SBB(state, Register.C)),
            [0x9a] = new Opcode("SBB D",     1,   state => Operations.SBB(state, Register.D)),
            [0x9b] = new Opcode("SBB E",     1,   state => Operations.SBB(state, Register.E)),
            [0x9c] = new Opcode("SBB H",     1,   state => Operations.SBB(state, Register.H)),
            [0x9d] = new Opcode("SBB L",     1,   state => Operations.SBB(state, Register.L)),
            [0x9e] = new Opcode("SBB M",     1,   state => Operations.SBB(state, Register.M)),
            [0x9f] = new Opcode("SBB A",     1,   state => Operations.SBB(state, Register.A)),
            [0xa0] = new Opcode("ANA B",     1,   state => Operations.ANA(state, Register.B)),
            [0xa1] = new Opcode("ANA C",     1,   state => Operations.ANA(state, Register.C)),
            [0xa2] = new Opcode("ANA D",     1,   state => Operations.ANA(state, Register.D)),
            [0xa3] = new Opcode("ANA E",     1,   state => Operations.ANA(state, Register.E)),
            [0xa4] = new Opcode("ANA H",     1,   state => Operations.ANA(state, Register.H)),
            [0xa5] = new Opcode("ANA L",     1,   state => Operations.ANA(state, Register.L)),
            [0xa6] = new Opcode("ANA M",     1,   state => Operations.ANA(state, Register.M)),
            [0xa7] = new Opcode("ANA A",     1,   state => Operations.ANA(state, Register.A)),
            [0xa8] = new Opcode("XRA B",     1,   state => Operations.XRA(state, Register.B)),
            [0xa9] = new Opcode("XRA C",     1,   state => Operations.XRA(state, Register.C)),
            [0xaa] = new Opcode("XRA D",     1,   state => Operations.XRA(state, Register.D)),
            [0xab] = new Opcode("XRA E",     1,   state => Operations.XRA(state, Register.E)),
            [0xac] = new Opcode("XRA H",     1,   state => Operations.XRA(state, Register.H)),
            [0xad] = new Opcode("XRA L",     1,   state => Operations.XRA(state, Register.L)),
            [0xae] = new Opcode("XRA M",     1,   state => Operations.XRA(state, Register.M)),
            [0xaf] = new Opcode("XRA A",     1,   state => Operations.XRA(state, Register.A)),
            [0xb0] = new Opcode("ORA B",     1,   state => Operations.ORA(state, Register.B)),
            [0xb1] = new Opcode("ORA C",     1,   state => Operations.ORA(state, Register.C)),
            [0xb2] = new Opcode("ORA D",     1,   state => Operations.ORA(state, Register.D)),
            [0xb3] = new Opcode("ORA E",     1,   state => Operations.ORA(state, Register.E)),
            [0xb4] = new Opcode("ORA H",     1,   state => Operations.ORA(state, Register.H)),
            [0xb5] = new Opcode("ORA L",     1,   state => Operations.ORA(state, Register.L)),
            [0xb6] = new Opcode("ORA M",     1,   state => Operations.ORA(state, Register.M)),
            [0xb7] = new Opcode("ORA A",     1,   state => Operations.ORA(state, Register.A)),
            [0xb8] = new Opcode("CMP B",     1,   state => Operations.CMP(state, Register.B)),
            [0xb9] = new Opcode("CMP C",     1,   state => Operations.CMP(state, Register.C)),
            [0xba] = new Opcode("CMP D",     1,   state => Operations.CMP(state, Register.D)),
            [0xbb] = new Opcode("CMP E",     1,   state => Operations.CMP(state, Register.E)),
            [0xbc] = new Opcode("CMP H",     1,   state => Operations.CMP(state, Register.H)),
            [0xbd] = new Opcode("CMP L",     1,   state => Operations.CMP(state, Register.L)),
            [0xbe] = new Opcode("CMP M",     1,   state => Operations.CMP(state, Register.M)),
            [0xbf] = new Opcode("CMP A",     1,   state => Operations.CMP(state, Register.A)),
            [0xc0] = new Opcode("RNZ",       1,   state => Operations.RET(state, !state.Flags.Zero)),
            [0xc1] = new Opcode("POP B",     1,   state => Operations.POP(state, RegisterPair.BC)),
            [0xc2] = new Opcode("JNZ adr",   3,   state => Operations.JMP(state, !state.Flags.Zero)),
            [0xc3] = new Opcode("JMP adr",   3,   state => Operations.JMP(state)),
            [0xc4] = new Opcode("CNZ adr",   3,   state => Operations.CALL(state, !state.Flags.Zero)),
            [0xc5] = new Opcode("PUSH B",    1,   state => Operations.PUSH(state, RegisterPair.BC)),
            [0xc6] = new Opcode("ADI",       2,   state => Operations.ADI(state)),
            [0xc7] = new Opcode("RST 0",     1,   state => Operations.RST(state, 0)),
            [0xc8] = new Opcode("RZ",        1,   state => Operations.RET(state, state.Flags.Zero)),
            [0xc9] = new Opcode("RET",       1,   state => Operations.RET(state)),
            [0xca] = new Opcode("JZ adr",    3,   state => Operations.JMP(state, state.Flags.Zero)),
            [0xcb] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0xcc] = new Opcode("CZ adr",    3,   state => Operations.CALL(state, state.Flags.Zero)),
            [0xcd] = new Opcode("CALL adr",  3,   state => Operations.CALL(state)),
            [0xce] = new Opcode("ACI",       2,   state => Operations.ACI(state)),
            [0xcf] = new Opcode("RST 1",     1,   state => Operations.RST(state, 1)),
            [0xd0] = new Opcode("RNC",       1,   state => Operations.RET(state, !state.Flags.Carry)),
            [0xd1] = new Opcode("POP D",     1,   state => Operations.POP(state, RegisterPair.DE)),
            [0xd2] = new Opcode("JNC adr",   3,   state => Operations.JMP(state, !state.Flags.Carry)),
            [0xd3] = new Opcode("OUT",       2,   state => Operations.OUT(state)),
            [0xd4] = new Opcode("CNC adr",   3,   state => Operations.CALL(state, !state.Flags.Carry)),
            [0xd5] = new Opcode("PUSH D",    1,   state => Operations.PUSH(state, RegisterPair.DE)),
            [0xd6] = new Opcode("SUI",       2,   state => Operations.SUI(state)),
            [0xd7] = new Opcode("RST 2",     1,   state => Operations.RST(state, 2)),
            [0xd8] = new Opcode("RC",        1,   state => Operations.RET(state, state.Flags.Carry)),
            [0xd9] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0xda] = new Opcode("JC adr",    3,   state => Operations.JMP(state, state.Flags.Carry)),
            [0xdb] = new Opcode("IN",        2,   state => Operations.IN(state)),
            [0xdc] = new Opcode("CC adr",    3,   state => Operations.CALL(state, state.Flags.Carry)),
            [0xdd] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0xde] = new Opcode("SBI",       2,   state => Operations.SBI(state)),
            [0xdf] = new Opcode("RST 3",     1,   state => Operations.RST(state, 3)),
            [0xe0] = new Opcode("RPO",       1,   state => Operations.RET(state, !state.Flags.Parity)),
            [0xe1] = new Opcode("POP H",     1,   state => Operations.POP(state, RegisterPair.HL)),
            [0xe2] = new Opcode("JPO adr",   3,   state => Operations.JMP(state, !state.Flags.Parity)),
            [0xe3] = new Opcode("XTHL",      1,   state => Operations.XTHL(state)),
            [0xe4] = new Opcode("CPO adr",   3,   state => Operations.CALL(state, !state.Flags.Parity)),
            [0xe5] = new Opcode("PUSH H",    1,   state => Operations.PUSH(state, RegisterPair.HL)),
            [0xe6] = new Opcode("ANI",       2,   state => Operations.ANI(state)),
            [0xe7] = new Opcode("RST 4",     1,   state => Operations.RST(state, 4)),
            [0xe8] = new Opcode("RPE",       1,   state => Operations.RET(state, state.Flags.Parity)),
            [0xe9] = new Opcode("PCHL",      1,   state => Operations.PCHL(state)),
            [0xea] = new Opcode("JPE adr",   3,   state => Operations.JMP(state, state.Flags.Parity)),
            [0xeb] = new Opcode("XCHG",      1,   state => Operations.XCHG(state)),
            [0xec] = new Opcode("CPE adr",   3,   state => Operations.CALL(state, state.Flags.Parity)),
            [0xed] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0xee] = new Opcode("XRI",       2,   state => Operations.XRI(state)),
            [0xef] = new Opcode("RST 5",     1,   state => Operations.RST(state, 5)),
            [0xf0] = new Opcode("RP",        1,   state => Operations.RET(state, !state.Flags.Sign)),
            [0xf1] = new Opcode("POP PSW",    1,   state => Operations.POP_PSW(state)),
            [0xf2] = new Opcode("JP adr",    3,   state => Operations.JMP(state, !state.Flags.Sign)),
            [0xf3] = new Opcode("DI",        1,   state => Operations.DI(state)),
            [0xf4] = new Opcode("CP adr",    3,   state => Operations.CALL(state, !state.Flags.Sign)),
            [0xf5] = new Opcode("PUSH PSW",  1,   state => Operations.PUSH_PSW(state)),
            [0xf6] = new Opcode("ORI",       2,   state => Operations.ORI(state)),
            [0xf7] = new Opcode("RST 6",     1,   state => Operations.RST(state, 6)),
            [0xf8] = new Opcode("RM",        1,   state => Operations.RET(state, state.Flags.Sign)),
            [0xf9] = new Opcode("SPHL",      1,   state => Operations.SPHL(state)),
            [0xfa] = new Opcode("JM adr",    3,   state => Operations.JMP(state, state.Flags.Sign)),
            [0xfb] = new Opcode("EI",        1,   state => Operations.EI(state)),
            [0xfc] = new Opcode("CM adr",    3,   state => Operations.CALL(state, state.Flags.Sign)),
            [0xfd] = new Opcode("NOP",       1,   state => Operations.NOP(state)),
            [0xfe] = new Opcode("CPI",       2,   state => Operations.CPI(state)),
            [0xff] = new Opcode("RST 7",     1,   state => Operations.RST(state, 7))
        };

        public Intel8080()
            : this(0x10000)
        { 
        }

        public Intel8080(int memorySize)
        {
            State = new CpuState(new Memory(memorySize));
        }

        public void LoadRom(string path)
        {
            var rom = File.ReadAllBytes(path);
            State.Memory.Load(rom, 0);
        }

        public void LoadCpuTestRom(string path)
        {
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var rom = new byte[file.Length + 0x100];
                file.Read(rom, 0x100, (int)file.Length);

                // All test binaries start at 0x0100.
                State.PC = 0x0100;

                // inject "out 0,a" at 0x0000 (signal to stop the test)
                rom[0x0000] = 0xD3;
                rom[0x0001] = 0x00;

                // inject "out 1,a" at 0x0005 (signal to output some characters)
                rom[0x0005] = 0xD3;
                rom[0x0006] = 0x01;
                rom[0x0007] = 0xC9;

                State.Memory.Load(rom, 0);
            }
        }

        public int Step()
        {
            var code = State.Memory.ReadInt8(State.PC);
            var oldPC = State.PC;

            if (opcodes.TryGetValue(code, out var opcode))
            {
                //Thread.Sleep(2);
                var cycles = opcode.Execute(State);
                //Console.Write($"{State.PC:x4}\t{opcode.Instruction}\t(0x{code:x2})\tA:{State.A:x2},B:{State.B:x2},C:{State.C:x2},D:{State.D:x2},E:{State.E:x2},H:{State.H},L:{State.L},M:{State.M:x2},SP:{State.Stack.SP:x4},B:{State.BC:x4},DE:{State.DE:x4},HL:{State.HL:x4}");
                //Console.WriteLine($" - Flags: Z:{State.Flags.Zero.ToBit()},S:{State.Flags.Sign.ToBit()},P:{State.Flags.Parity.ToBit()},C:{State.Flags.Carry.ToBit()},AC:{State.Flags.AuxCarry.ToBit()}");
                if (State.PC == oldPC)
                {
                    State.PC += opcode.Size;
                }
                return cycles;
            }
            else
            {
                throw new NotImplementedException($"Opcode not implemented: 0x{code:x2}");
            }
        }
    }
}
