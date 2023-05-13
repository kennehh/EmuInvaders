namespace EmuInvaders.Machine
{
    internal class ShiftRegister
    {
        public byte Shift0 { get; set; }
        public byte Shift1 { get; set; }
        public byte ShiftOffset { get; set; }

        public byte DoBitShift()
        {
            var v = (ushort)((Shift1 << 8) | Shift0);
            return (byte)((v >> (8 - ShiftOffset)) & 0xff);
        }
    }
}