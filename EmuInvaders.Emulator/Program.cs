using EmuInvaders.Machine;
using SDL2;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace EmuInvaders.Emulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var window = new Window())
            {
                window.Open();
            }
        }
    }
}