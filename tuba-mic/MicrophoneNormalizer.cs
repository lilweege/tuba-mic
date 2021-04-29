using NAudio.Mixer;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace tuba_mic
{
    // NAudio is an open source .NET audio library written by Mark Heath
    // https://github.com/naudio/NAudio
    // this article written by him has a large portion of what I am trying to do already outlined
    // https://channel9.msdn.com/coding4fun/articles/NET-Voice-Recorder

    class MicrophoneNormalizer
    {
        SampleAggregator processor;
        WaveInEvent waveIn;
        UnsignedMixerControl volumeControl;
        float lastPeak;
        RunningMean avgPeak;
        float sampleNorm = 1;

        static uint mapLevel(uint val /*0 - 100*/) { return val * ((1 << 16) - 1) / 100; } /*0 - 65535*/

        public MicrophoneNormalizer()
        {
            /*
            // check devices
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                Console.WriteLine("Device {0}: {1}, {2} channels",
                    waveInDevice, deviceInfo.ProductName, deviceInfo.Channels);
            }
            if (waveInDevices == 0)
            {
                Console.WriteLine("No input devices found.");
                return;
            }
            */


            // TODO: get device num from input ...
            // for now simply assume the desired
            // device will show up in the first position
            int selectedDevice = 0;

            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = selectedDevice;
            waveIn.DataAvailable += waveIn_DataAvailable;

            if (!TryGetVolumeControl())
            {
                Console.WriteLine("Could not get volume control");
                return;
            }

            processor = new SampleAggregator();
            processor.NotificationCount = 50;
            processor.MaximumCalculated += processor_MaximumCalculated;
            avgPeak = new RunningMean(50);

            waveIn.WaveFormat = new WaveFormat(8000, 1); // 8kHz sample rate, mono channel
            // fidelity doesn't matter because we only need volume level
            waveIn.StartRecording();
            // supposedly, data is not actually saved anywhere
            // although I am skeptical of a potential memory leak
        }
        
        private void setVolume(uint val)
        {
            volumeControl.Value = mapLevel(val);
            sampleNorm = 100f / (float)val;
        }

        private void doNormalization()
        {
            // this is not fully accurate, and should
            // be tuned for the specific use case.
            // of course this still isn't perfect

            if (lastPeak >= 0.7)
            {
                setVolume(60);
            }
            else if (avgPeak.avg() < 0.2)
            {
                setVolume(100);
            }
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) |
                                        e.Buffer[index + 0]);
                float sample32 = sample / 32768f;
                processor.Add(sample32);
            }
        }

        private void processor_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            /*
            Console.Clear();
            Console.WriteLine(lastPeak);
            for (int x = 0; x < lastPeak * 100; ++x)
            {
                Console.Write("#");
            }
            */
            avgPeak.add(lastPeak * sampleNorm);
            doNormalization();
        }

        private bool TryGetVolumeControl()
        {
            var mixerLine = new MixerLine((IntPtr)waveIn.DeviceNumber, 0, MixerFlags.WaveIn);
            foreach (var control in mixerLine.Controls)
            {
                if (control.ControlType == MixerControlType.Volume)
                {
                    volumeControl = control as UnsignedMixerControl;
                    return true;
                }
            }
            return false;
        }
    }
}
