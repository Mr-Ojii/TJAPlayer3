using System;
using System.Collections.Generic;
using System.Text;
using ManagedBass;
using ManagedBass.Mix;

namespace FDK.BassMixExtension
{
    public static class BassMixExtensions
    {
        public static bool ChannelPlay(int hHandle)
        {
            return ((int)BassMix.ChannelFlags(hHandle, 0, BassFlags.MixerChanPause) != -1);
        }

        public static bool ChannelPause(int hHandle)
        {
            return ((int)BassMix.ChannelFlags(hHandle, BassFlags.MixerChanPause, BassFlags.MixerChanPause) != -1);
        }

        public static bool ChannelIsPlaying(int hHandle)
        {
            return !BassMix.ChannelHasFlag(hHandle, BassFlags.MixerChanPause);
        }
    }
}
