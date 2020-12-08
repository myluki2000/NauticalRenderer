using System;

namespace WindowsRenderer
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new NauticalRenderer.Main(new WindowsResourceManager(), new WindowsSettingsManager()))
                game.Run();
        }
    }
}
