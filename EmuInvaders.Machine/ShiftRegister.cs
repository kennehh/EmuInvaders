namespace EmuInvaders.Machine
{
    internal class ShiftRegister
    {
        private byte shift0 = 0;
        private byte shift1 = 0;
        private byte offset = 0;

        public byte Read()
        {
            var v = (ushort)((shift1 << 8) | shift0);
            return (byte)((v >> (8 - offset)) & 0xff);
        }

        public void WriteOffset(byte value)
        {
            offset = value;
        }

        public void Write(byte value)
        {
            shift0 = shift1;
            shift1 = value;
        }
    }
}