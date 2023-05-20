using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reverber
{
    static class ReverbModificator
    {   
        public static Wave Modify(Wave wave,int delayinMilliSeconds,float decayFactor, int sampleRate, float mixPercent)
        {
			Wave combFilterSamples1 = CombFilter(wave, delayinMilliSeconds, decayFactor, sampleRate);
			Wave combFilterSamples2 = CombFilter(wave, (delayinMilliSeconds - 11.73f), (decayFactor - 0.1313f), sampleRate);
			Wave combFilterSamples3 = CombFilter(wave, (delayinMilliSeconds + 19.31f), (decayFactor - 0.2743f), sampleRate);
			Wave combFilterSamples4 = CombFilter(wave, (delayinMilliSeconds - 7.97f), (decayFactor - 0.31f), sampleRate);

			Wave outputComb = new Wave(wave.Length, wave.Channels, wave.OriginalSize);
			for (int i = 0; i < outputComb.Length; i++)
				for(int channel = 0; channel < outputComb.Channels; channel++)
				{
					float sample1 = combFilterSamples1.GetFloat(i, channel);
					float sample2 = combFilterSamples2.GetFloat(i, channel);
					float sample3 = combFilterSamples3.GetFloat(i, channel);
					float sample4 = combFilterSamples4.GetFloat(i, channel);

					outputComb.SetFloat(i, channel, sample1 + sample2 + sample3 + sample3 + sample4);
				}

			//Algorithm for Dry/Wet Mix in the output audio
			Wave mixAudio = new Wave(wave.Length, wave.Channels, wave.OriginalSize);
			for (int i = 0; i < mixAudio.Length; i++)
				for (int channel = 0; channel < mixAudio.Channels; channel++)
				{
					float originalSample = wave.GetFloat(i, channel);
					float combSample = outputComb.GetFloat(i, channel);
					mixAudio.SetFloat(i, channel, (1 - mixPercent) * originalSample + (mixPercent * combSample));
				}

			Wave allPassFilterSamples1 = AllPassFilter(mixAudio, sampleRate);
			Wave allPassFilterSamples2 = AllPassFilter(allPassFilterSamples1, sampleRate);

			return allPassFilterSamples2;
        }

		private static Wave CombFilter(Wave wave, float delayinMilliSeconds, float decayFactor, int sampleRate) 
		{
			int delaySamples = (int)(delayinMilliSeconds * (sampleRate / 1000));

			Wave combFilterSamples = new Wave(wave);

			for (int i = 0; i < wave.Length - delaySamples; i++)
			{
				for(int channel = 0; channel < wave.Channels; channel++)
                {
					float toBeDelayed = combFilterSamples.GetFloat(i + delaySamples, channel);
					float delayee = combFilterSamples.GetFloat(i, channel);

					combFilterSamples.SetFloat(i + delaySamples, channel, toBeDelayed + delayee * decayFactor);
				}
					
			}
			return combFilterSamples;
		}

		private static Wave AllPassFilter(Wave wave, float sampleRate)
		{
			int delaySamples = (int)((float)89.27f * (sampleRate / 1000));
			Wave allPassFilterSamples = new Wave(wave.Length, wave.Channels, wave.OriginalSize);
			float decayFactor = 0.131f;

			for (int i = 0; i < wave.Length; i++)
			{
				for (int channel = 0; channel < wave.Channels; channel++)
				{
					float waveCurrent = wave.GetFloat(i, channel);

					allPassFilterSamples.SetFloat(i, channel, waveCurrent);

					if (i - delaySamples >= 0)
                    {
						float allPassFilterSampleDelayed = allPassFilterSamples.GetFloat(i - delaySamples, channel);
						allPassFilterSamples.SetFloat(i, channel,
							allPassFilterSamples.GetFloat(i, channel) + -decayFactor * allPassFilterSampleDelayed);

					}

					if (i - delaySamples >= 1)
					{
						float allPassFilterSampleDelayed = allPassFilterSamples.GetFloat(i + 20 - delaySamples, channel);
						allPassFilterSamples.SetFloat(i, channel,
							allPassFilterSamples.GetFloat(i, channel) + decayFactor * allPassFilterSampleDelayed);
					}
				}
			}


			for (int channel = 0; channel < wave.Channels; channel++)
			{
				float value = allPassFilterSamples.GetFloat(0, channel);
				float max = 0.0f;

				for (int i = 0; i < allPassFilterSamples.Length; i++)
				{
					if (Math.Abs(allPassFilterSamples.GetFloat(i, channel)) > max)
						max = Math.Abs(allPassFilterSamples.GetFloat(i, channel));
				}

				for (int i = 0; i < allPassFilterSamples.Length; i++)
				{
					float currentValue = allPassFilterSamples.GetFloat(i, channel);
					value = ((value + (currentValue - value)) / max);

					allPassFilterSamples.SetFloat(i, channel, value);
				}
			}
			return allPassFilterSamples;
		}
	}
}
