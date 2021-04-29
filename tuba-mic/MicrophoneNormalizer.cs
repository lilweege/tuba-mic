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
    class MicrophoneNormalizer
    {
        SampleAggregator maxProcessor;
        WaveInEvent waveIn;
        UnsignedMixerControl volumeControl;
        float lastPeak;
        RunningMean avgPeak;

        bool isReduced = false;

        static uint mapLevel(uint val /*0 - 100*/) { return val * ((1 << 16) - 1) / 100; } /*0 - 65535*/

        public MicrophoneNormalizer()
        {
            Console.WriteLine("Release 0.0.6");
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                Console.WriteLine("Device {0}: {1}, {2} channels",
                    waveInDevice, deviceInfo.ProductName, deviceInfo.Channels);
            }

            // get device num from input ...
            int selectedDevice = 0;

            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = selectedDevice;
            waveIn.DataAvailable += waveIn_DataAvailable;

            if (!TryGetVolumeControl())
            {
                Console.WriteLine("Could not get volume control");
                return;
            }
            Console.Write("Got volume control handle for device ");
            Console.WriteLine(waveIn.DeviceNumber);

            maxProcessor = new SampleAggregator();
            maxProcessor.NotificationCount = 50;
            maxProcessor.MaximumCalculated += MaximumCalculated;
            avgPeak = new RunningMean(50);

            int sampleRate = 8000; // 8 kHz
            int channels = 1; // mono
            // doesn't matter because we only need volume level
            waveIn.WaveFormat = new WaveFormat(sampleRate, channels);
            waveIn.StartRecording();
        }

        void doNormalization()
        {
            if (lastPeak < 0.2)
            {
                volumeControl.Value = mapLevel(100);
                isReduced = false;
            }
            else
            {
                volumeControl.Value = mapLevel(60);
                isReduced = true;
            }
        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) |
                                        e.Buffer[index + 0]);
                float sample32 = sample / 32768f;
                maxProcessor.Add(sample32);
            }
        }

        void MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));

//            Console.Clear();
//            Console.WriteLine(lastPeak);
//            for (int x = 0; x < lastPeak * 100; ++x)
//            {
//                Console.Write("#");
//            }
            if (isReduced)
            {
                lastPeak /= 0.6f;
            }
            avgPeak.add(lastPeak);
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
