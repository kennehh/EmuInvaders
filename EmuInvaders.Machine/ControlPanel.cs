namespace EmuInvaders.Machine
{
    public class ControlPanel
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

        public void ButtonDown(Button key)
        {
            switch (key)
            {
                case Button.Coin:
                    InputValue |= 1 << 0;
                    break;
                case Button.Start:
                    InputValue |= 1 << 2;
                    break;
                case Button.Fire:
                    InputValue |= 1 << 4;
                    break;
                case Button.Left:
                    InputValue |= 1 << 5;
                    break;
                case Button.Right:
                    InputValue |= 1 << 6;
                    break;
            }
        }

        public void ButtonUp(Button key)
        {
            unchecked
            {
                switch (key)
                {
                    case Button.Coin:
                        InputValue &= (byte)(~(1 << 0));
                        break;
                    case Button.Start:
                        InputValue &= (byte)(~(1 << 2));
                        break;
                    case Button.Fire:
                        InputValue &= (byte)(~(1 << 4));
                        break;
                    case Button.Left:
                        InputValue &= (byte)(~(1 << 5));
                        break;
                    case Button.Right:
                        InputValue &= (byte)(~(1 << 6));
                        break;
                }
            }
        }
    }

    public enum Button
    {
        Coin,
        Left,
        Right,
        Fire,
        Start
    }
}
