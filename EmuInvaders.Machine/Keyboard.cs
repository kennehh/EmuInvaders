using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Machine
{
    public class Keyboard
    {
        //  Port 1
        //  bit 0 = CREDIT (1 if deposit)
        //  bit 1 = 2P start (1 if pressed)
        //  bit 2 = 1P start (1 if pressed)
        //  bit 3 = Always 1
        //  bit 4 = 1P shot (1 if pressed)
        //  bit 5 = 1P left (1 if pressed)
        //  bit 6 = 1P right (1 if pressed)
        //  bit 7 = Not connected
        internal byte InputValue { get; private set; } = 0b00001000;

        public void KeyDown(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Coin:
                    InputValue |= 1 << 0;
                    break;
                case KeyCode.Left:
                    InputValue |= 1 << 5;
                    break;
                case KeyCode.Right:
                    InputValue |= 1 << 6;
                    break;
                case KeyCode.Fire:
                    InputValue |= 1 << 4;
                    break;
                case KeyCode.Start:
                    InputValue |= 1 << 2;
                    break;
            }
        }

        public void KeyUp(KeyCode key)
        {
            unchecked
            {
                switch (key)
                {
                    case KeyCode.Coin:
                        InputValue &= (byte)(~(1 << 0));
                        break;
                    case KeyCode.Left:
                        InputValue &= (byte)(~(1 << 5));
                        break;
                    case KeyCode.Right:
                        InputValue &= (byte)(~(1 << 6));
                        break;
                    case KeyCode.Fire:
                        InputValue &= (byte)(~(1 << 4));
                        break;
                    case KeyCode.Start:
                        InputValue &= (byte)(~(1 << 2));
                        break;
                }
            }
        }
    }

    public enum KeyCode
    {
        Coin,
        Left,
        Right,
        Fire,
        Start
    }
}
