using EmuInvaders.Cpu;
using System;

namespace EmuInvaders.Machine
{
    public class SpaceInvadersMachine
    {
        private readonly Intel8080 cpu;

        public SpaceInvadersMachine()
        {
            cpu = new Intel8080();
        }

        public void Initialise()
        {

        }

        private void LoadRom()
        {

        }
    }
}
