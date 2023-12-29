using ManagedBass;
using ManagedBass.Mix;

namespace FDK.BassMixExtension;

public static class BassMixExtensions
{
    public static bool ChannelPlay(int hHandle)
        => BassMix.ChannelRemoveFlag(hHandle, BassFlags.MixerChanPause);

    public static bool ChannelPause(int hHandle)
        => BassMix.ChannelAddFlag(hHandle, BassFlags.MixerChanPause);

    public static bool ChannelIsPlaying(int hHandle)
        => !BassMix.ChannelHasFlag(hHandle, BassFlags.MixerChanPause);
}
