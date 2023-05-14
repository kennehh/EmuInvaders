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
                    InputValue |= 0b00000001;
                    break;
                case KeyCode.Left:
                    InputValue |= 0b00100000;
                    break;
                case KeyCode.Right:
                    InputValue |= 0b01000000;
                    break;
                case KeyCode.Fire:
                    InputValue |= 0b00010000;
                    break;
                case KeyCode.Start:
                    InputValue |= 0b00000100;
                    break;
            }
        }

        public void KeyUp(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Coin:
                    InputValue &= 0b00000001;
                    break;
                case KeyCode.Left:
                    InputValue &= 0b00100000;
                    break;
                case KeyCode.Right:
                    InputValue &= 0b01000000;
                    break;
                case KeyCode.Fire:
                    InputValue &= 0b00010000;
                    break;
                case KeyCode.Start:
                    InputValue &= 0b00000100;
                    break;
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
