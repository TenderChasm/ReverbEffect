using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Reverber
{
    class Wave
    {
        public int Size { get => channeledPCM.Length * sizeof(float); }
        public int Length { get => channeledPCM.GetLength(0); }
        public int Channels { get => channeledPCM.GetLength(1); }

        public float[,] channeledPCM { get; private set; }

        public int OriginalSize;
        
        public Wave(Span<byte> pcm, int numberOfChannels,int numberOfBits)
        {
            OriginalSize = numberOfBits / 8;

            int arrayLen = pcm.Length / OriginalSize / numberOfChannels;

            if (pcm.Length % numberOfChannels != 0)
                throw new ArgumentException("Incorrect number of channels");

            channeledPCM = new float[arrayLen, numberOfChannels];

            byte[] buffer = new byte[OriginalSize];

            for (int timestep = 0; timestep < arrayLen; timestep++)
            {
                for (int channel = 0; channel < numberOfChannels; channel++)
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = MemoryMarshal.Read<byte>(pcm);
                        pcm = pcm.Slice(1);
                    }

                        channeledPCM[timestep, channel] = ReadAndConvertToFloat(buffer, OriginalSize);
                }

            }
                    
        }

        public Wave(Wave wave)
        {
            channeledPCM = wave.channeledPCM.Clone() as float[,];
            OriginalSize = OriginalSize;
        }

        public Wave(int length, int channels, int originalSize)
        {
            OriginalSize = originalSize;
            channeledPCM = new float[length, channels];
        }

        static float GetMaxValueOfType(int size)
        {
            return MathF.Pow(2, size * 8 - 1);
        }

        float ReadAndConvertToFloat(byte[] buffer, int size)
        {

            float result;

            short test = BitConverter.ToInt16(buffer, 0);
            float testic = GetMaxValueOfType(2);

            result = size switch
            {
                1 => buffer[0] / (float) GetMaxValueOfType(1),
                2 => BitConverter.ToInt16(buffer, 0) / (float)GetMaxValueOfType(2),
                4 => BitConverter.ToSingle(buffer, 0)
            };

            return result;
        }

        byte[] ConvertToOriginalType(float value, int size)
        {
            //short kek = BitConverter.GetBytes(value * GetMaxValueOfType(size)).Take(2).ToArray();

            return size switch
            {
                1 => BitConverter.GetBytes((byte)(value * GetMaxValueOfType(size))).Take(1).ToArray(),
                2 => BitConverter.GetBytes((short)(value * GetMaxValueOfType(size))).Take(2).ToArray(),
                4 => BitConverter.GetBytes(value).Take(4).ToArray(),
            };
        }

        public float GetFloat(int index, int channel)
        {
                return channeledPCM[index, channel];
        }

        public void SetFloat(int index, int channel, float value)
        {
                channeledPCM[index, channel] = value;
        }

        public unsafe void Export(Stream outStream)
        {
            BinaryWriter writer = new BinaryWriter(outStream);

            for (int sample = 0; sample < Length; sample++)
                for (int channel = 0; channel < Channels; channel++)
                    writer.Write(ConvertToOriginalType(channeledPCM[sample, channel], OriginalSize));

            writer.Flush();
        }
    }
}
