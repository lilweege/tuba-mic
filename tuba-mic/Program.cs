using System;
using System.Threading;


namespace tuba_mic
{
    class Program
    {
        // https://stackoverflow.com/q/2586612/12637867

        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            new MicrophoneNormalizer();
            quitEvent.WaitOne();
        }
    }

}
