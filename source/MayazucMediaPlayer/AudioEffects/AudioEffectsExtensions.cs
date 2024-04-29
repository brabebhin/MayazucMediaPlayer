using FluentResults;
using MayazucMediaPlayer.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.AudioEffects
{
    public static class AudioEffectsExtensions
    {
        public static double GetOctaves(double f1, double f2)
        {
            return Math.Log(f2 / f1, 2);
        }

        public static async Task<Result<AudioEqualizerPreset>> CreateNewPresetAsync(bool loadCurrentAmps, EqualizerConfiguration configuration)
        {
            AudioEqualizerPreset preset = new AudioEqualizerPreset();
            preset.IsEnabled = true;
            StringInputDialog diag = new StringInputDialog("Add new preset", "Input preset name");
            await ContentDialogService.Instance.ShowAsync(diag);
            if (!string.IsNullOrWhiteSpace(diag.Result))
            {
                preset.PresetName = diag.Result;

                if (await SetAmplificationsAsync(preset, loadCurrentAmps, configuration))
                {
                    return Result.Ok(preset);
                    //lsvPresets.ScrollIntoView(preset);
                }
            }

            return Result.Fail("Could not create preset");
        }

        public static async Task<bool> SetAmplificationsAsync(this AudioEqualizerPreset preset, bool loadCurrentAmps, EqualizerConfiguration configuration)
        {
            AmplificationsPicker diag = new AmplificationsPicker(configuration);
            diag.LoadCurrentAmps = loadCurrentAmps;
            diag.InitialPreset = preset;
            await ContentDialogService.Instance.ShowAsync(diag);

            var results = diag.Results;
            if (results == null) return false;
            if (results.Succeded)
            {
                preset.SetAmplifications(results.AmplificationBands.Select(x => x.FrequencyAmplification));
            }

            return results.Succeded;
        }
    }
}
