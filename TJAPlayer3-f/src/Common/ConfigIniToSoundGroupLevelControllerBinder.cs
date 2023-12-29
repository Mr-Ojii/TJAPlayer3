using FDK;

namespace TJAPlayer3;

/// <summary>
/// The ConfigIniToSoundGroupLevelControllerBinder allows for updated sound
/// group level values, and keyboard sound level adjustment increment
/// values, to flow between CConfigIni and the SoundGroupLevelController
/// without either of those two classes being aware of one another.
/// See those classes properties, methods, and events for more details.
/// </summary>
internal static class ConfigIniToSoundGroupLevelControllerBinder
{
    internal static void Bind(CConfigToml configToml, SoundGroupLevelController soundGroupLevelController)
    {
        soundGroupLevelController.SetLevel(ESoundGroup.SoundEffect, configToml.Sound.SoundEffectLevel);
        soundGroupLevelController.SetLevel(ESoundGroup.Voice, configToml.Sound.VoiceLevel);
        soundGroupLevelController.SetLevel(ESoundGroup.SongPreview, configToml.Sound.SongPreviewLevel);
        soundGroupLevelController.SetLevel(ESoundGroup.SongPlayback, configToml.Sound.SongPlaybackLevel);
        soundGroupLevelController.SetKeyboardSoundLevelIncrement(configToml.Sound.KeyboardSoundLevelIncrement);

        configToml.Sound.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(CConfigToml.Sound.SoundEffectLevel):
                    soundGroupLevelController.SetLevel(ESoundGroup.SoundEffect, configToml.Sound.SoundEffectLevel);
                    break;
                case nameof(CConfigToml.Sound.VoiceLevel):
                    soundGroupLevelController.SetLevel(ESoundGroup.Voice, configToml.Sound.VoiceLevel);
                    break;
                case nameof(CConfigToml.Sound.SongPreviewLevel):
                    soundGroupLevelController.SetLevel(ESoundGroup.SongPreview, configToml.Sound.SongPreviewLevel);
                    break;
                case nameof(CConfigToml.Sound.SongPlaybackLevel):
                    soundGroupLevelController.SetLevel(ESoundGroup.SongPlayback, configToml.Sound.SongPlaybackLevel);
                    break;
                case nameof(CConfigToml.Sound.KeyboardSoundLevelIncrement):
                    soundGroupLevelController.SetKeyboardSoundLevelIncrement(configToml.Sound.KeyboardSoundLevelIncrement);
                    break;
            }
        };

        soundGroupLevelController.LevelChanged += (sender, args) =>
        {
            switch (args.SoundGroup)
            {
                case ESoundGroup.SoundEffect:
                    configToml.Sound.SoundEffectLevel = args.Level;
                    break;
                case ESoundGroup.Voice:
                    configToml.Sound.VoiceLevel = args.Level;
                    break;
                case ESoundGroup.SongPreview:
                    configToml.Sound.SongPreviewLevel = args.Level;
                    break;
                case ESoundGroup.SongPlayback:
                    configToml.Sound.SongPlaybackLevel = args.Level;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
    }
}
