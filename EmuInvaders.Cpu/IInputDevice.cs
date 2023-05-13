using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Cpu
{
    public interface IInputHandler
    {
        byte ProcessInput(byte port);
    }
}
