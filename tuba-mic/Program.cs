using System;
using System.Threading;

// https://github.com/naudio/NAudio
// https://channel9.msdn.com/coding4fun/articles/NET-Voice-Recorder
// https://stackoverflow.com/q/2586612/12637867

namespace tuba_mic
{
    class Program
    {
        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            new MicrophoneNormalizer();
            quitEvent.WaitOne();
        }
    }

}
