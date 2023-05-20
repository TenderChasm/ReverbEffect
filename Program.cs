using Reverber;
using System;
using System.IO;

namespace ReverbEffect
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream test = File.OpenRead("input.wav");
            WaveData testWav = new WaveData(test);
            testWav.PcmWave = ReverbModificator.Modify(testWav.PcmWave, 20, 0.8F, testWav.SampleRate, 0.8F);
            testWav.Export("ouput.wav");
        }
    }
}
