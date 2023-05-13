using System;
using System.Collections.Generic;
using System.Text;

namespace EmuInvaders.Cpu
{
    public interface IOutputHandler
    {
        void ProcessOutput(byte port, byte value);
    }
}
