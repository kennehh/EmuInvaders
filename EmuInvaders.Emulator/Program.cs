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