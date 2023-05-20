using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reverber
{
    class WaveData
    {
        public int NumberOfChannels { get; private set; }
        public int SampleRate { get; private set; }
        public int BitsPerSample { get; private set; }
        public int DataSize { get => PcmWave.Length * PcmWave.OriginalSize * PcmWave.Channels; }

        public byte[] HeaderChunks { get; private set; }

        public Wave PcmWave { get; set; }

        public WaveData(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            reader.BaseStream.Seek(8, SeekOrigin.Begin);
            char[] formatName = reader.ReadChars(4);
            if (new string(formatName) != "WAVE")
                throw new FormatException("Not wav file");

            reader.BaseStream.Seek(22, SeekOrigin.Begin);
            NumberOfChannels = reader.ReadInt16();

            reader.BaseStream.Seek(24, SeekOrigin.Begin);
            SampleRate = reader.ReadInt32();

            reader.BaseStream.Seek(34, SeekOrigin.Begin);
            BitsPerSample = reader.ReadInt16();


            int dataSectionAddress = 0;
            reader.BaseStream.Seek(36, SeekOrigin.Begin);
            string toCheck = new string(reader.ReadChars(4));
            if (toCheck == "data")
                dataSectionAddress = 36;
            reader.BaseStream.Seek(190, SeekOrigin.Begin);
            toCheck = new string(reader.ReadChars(4));
            if (toCheck == "data")
                dataSectionAddress = 190;

            if (dataSectionAddress == 0)
                throw new FormatException("Corrupted Wav file");

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            HeaderChunks = reader.ReadBytes(dataSectionAddress);

            dataSectionAddress += 4;
            reader.BaseStream.Seek(dataSectionAddress, SeekOrigin.Begin);
            int dataSize = reader.ReadInt32();

            byte[] data = reader.ReadBytes(dataSize);

            PcmWave = new Wave(new Span<byte>(data), NumberOfChannels, BitsPerSample);

            /*PcmWave = BitsPerSample switch
            {
                8 => new Wave<byte>(new Span<byte>(data), NumberOfChannels),
                16 => new Wave<short>(new Span<byte>(data), NumberOfChannels),
                32 => new Wave<float>(new Span<byte>(data), NumberOfChannels)
            };*/

        }

        public unsafe void Export(string filename)
        {
            FileStream savedFile = File.Create(filename);
            BinaryWriter writer = new BinaryWriter(savedFile);

            writer.Write(HeaderChunks);

            writer.BaseStream.Seek(4, SeekOrigin.Begin);
            writer.Write(DataSize + HeaderChunks.Length);

            writer.BaseStream.Seek(HeaderChunks.Length, SeekOrigin.Begin);
            writer.Write("data".ToCharArray());

            writer.Write(DataSize);

            writer.Flush();

            PcmWave.Export(writer.BaseStream);

            savedFile.Close();
        }

        public static int TestSampleSize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            reader.BaseStream.Seek(8, SeekOrigin.Begin);
            char[] formatName = reader.ReadChars(4);
            if (new string(formatName) != "WAVE")
                throw new FormatException("Not wav file");

            reader.BaseStream.Seek(34, SeekOrigin.Begin);
            int bitsPerSample = reader.ReadInt16();
            return bitsPerSample;

        }
    }
}
