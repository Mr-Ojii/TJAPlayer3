using FDK;

namespace TJAPlayer3;

/// <summary>
/// The ConfigIniToSongGainControllerBinder allows for SONGVOL and/or other
/// properties related to the Gain levels applied to song preview and
/// playback, to be applied conditionally based on settings flowing from
/// ConfigIni. This binder class allows that to take place without either
/// ConfigIni or SongGainController having awareness of one another.
/// See those classes properties, methods, and events for more details. 
/// </summary>
internal static class ConfigIniToSongGainControllerBinder
{
    internal static void Bind(CConfigToml configToml, SongGainController songGainController)
    {
        songGainController.ApplyLoudnessMetadata = configToml.Sound.ApplyLoudnessMetadata;
        songGainController.TargetLoudness = new Lufs(configToml.Sound.TargetLoudness);
        songGainController.ApplySongVol = configToml.Sound.ApplySongVol;

        configToml.Sound.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(CConfigToml.CSoundConf.ApplyLoudnessMetadata):
                    songGainController.ApplyLoudnessMetadata = configToml.Sound.ApplyLoudnessMetadata;
                    break;
                case nameof(CConfigToml.CSoundConf.TargetLoudness):
                    songGainController.TargetLoudness = new Lufs(configToml.Sound.TargetLoudness);
                    break;
                case nameof(CConfigToml.CSoundConf.ApplySongVol):
                    songGainController.ApplySongVol = configToml.Sound.ApplySongVol;
                    break;
            }
        };
    }
}
