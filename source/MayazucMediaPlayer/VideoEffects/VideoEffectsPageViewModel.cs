using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.Services;
using MayazucNativeFramework;
using Microsoft.UI.Dispatching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MayazucMediaPlayer.VideoEffects
{
    public class VideoEffectsPageViewModel : ServiceBase
    {
        public event EventHandler<object> ProfileSliderValueChanged;

        public ObservableCollection<VideoEffectSliderProperty> EffectProperties
        {
            get;
            private set;
        } = new ObservableCollection<VideoEffectSliderProperty>();

        public ObservableHashSet<SavedColorProfile> SavedColorProfiles
        {
            get;
            private set;
        } = new ObservableHashSet<SavedColorProfile>();

        public IRelayCommand ResetDefault
        {
            get;
            private set;
        }

        public IRelayCommand<object> SaveColorProfile
        {
            get;
            private set;
        }

        public VideoEffectProcessorConfiguration EffectConfig
        {
            get;
            private set;
        }

        public bool? GrayscaleEnabled
        {
            get
            {
                return EffectConfig.GrayscaleEffect;
            }
            set
            {
                if (EffectConfig.GrayscaleEffect == value) return;
                EffectConfig.GrayscaleEffect = value.GetValueOrDefault(false);
                NotifyPropertyChanged(nameof(GrayscaleEnabled));
            }
        }

        public bool? BlurEnabled
        {
            get
            {
                return EffectConfig.BlurEffect;
            }
            set
            {
                if (EffectConfig.BlurEffect == value) return;

                EffectConfig.BlurEffect = value.GetValueOrDefault(false);
                NotifyPropertyChanged(nameof(BlurEnabled));
            }
        }


        public bool MasterSwitch
        {
            get
            {
                return EffectConfig.MasterSwitch;
            }
            set
            {
                if (EffectConfig.MasterSwitch == value) return;

                EffectConfig.MasterSwitch = value;
                NotifyPropertyChanged(nameof(MasterSwitch));
                foreach (var ep in EffectProperties)
                {
                    ep.Enabled = EffectConfig.MasterSwitch;
                }
            }
        }

        public VideoEffectsPageViewModel(DispatcherQueue dp, VideoEffectProcessorConfiguration effectConfig) : base(dp)
        {
            LoadSliders();
            ResetDefault = new RelayCommand(ResetDefaultValues);
            SaveColorProfile = new AsyncRelayCommand<object>(SaveColorProfileAsync);
            EffectConfig = effectConfig;
        }

        private void LoadSliders()
        {
            EffectProperties.Add(new VideoEffectSliderProperty("Brightness", 0.01f, 2.0f, 0.0f, -2.0f, 0.0f));
            EffectProperties.Add(new VideoEffectSliderProperty("Contrast", 0.01f, 2.0f, 0.0f, -2.0f, 0.0f));
            EffectProperties.Add(new VideoEffectSliderProperty("Saturation", 0.01, 2.0f, 0.0f, -2.0f, 0.0f));

            EffectProperties.Add(new VideoEffectSliderProperty("Sharpness", 0.01f, 10.0f, 0.0f, 0f, 0.0f));
            EffectProperties.Add(new VideoEffectSliderProperty("Temperature", 0.01f, 1.0f, 0.0f, -1.0f, 0.0f));
            EffectProperties.Add(new VideoEffectSliderProperty("Tint", 0.01f, 1.0f, 0.0f, -1.0f, 0.0f));

            foreach (var obs in EffectProperties)
            {
                obs.ValueChanged += Obs_ValueChanged;
            }
        }

        internal async Task DeleteColorProfileAsync(SavedColorProfile profileName)
        {
            try
            {
                var saveFolder = await LocalCache.LocalFolders.GetVideoColorProfilesFolder();
                var sfile = await saveFolder.TryGetItemAsync(profileName.Name);
                if (sfile != null)
                {
                    await sfile.DeleteAsync();
                    SavedColorProfiles.Remove(profileName);
                }
            }
            catch { }
        }

        internal async Task LoadSelecteProfileAsync(SavedColorProfile selectedItem)
        {
            try
            {
                var saveFolder = await LocalCache.LocalFolders.GetVideoColorProfilesFolder();
                var sfile = await saveFolder.TryGetItemAsync(selectedItem.Name);
                if (sfile != null)
                {
                    var file = sfile as StorageFile;
                    var json = await FileIO.ReadTextAsync(file);
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    foreach (var obj in EffectProperties)
                    {
                        obj.EffectPropertyValue = Convert.ToSingle((double)result[obj.EffectPropertyName]);
                    }
                }
                else
                {

                }
            }
            catch { }
        }

        public async Task LoadSavedColorProfiles()
        {
            await CreateDefaultColorProfiles();

            var saveFolder = await LocalCache.LocalFolders.GetVideoColorProfilesFolder();
            var result = await saveFolder.GetFilesAsync();
            var values = EffectProperties.ToList().AsEnumerable();

            foreach (var v in result.Select(y => y.Name).OrderBy(x => x))
            {
                SavedColorProfiles.Add(new SavedColorProfile(v));
            }
        }

        private async Task CreateDefaultColorProfiles()
        {
            SavedColorProfiles.Add(await CreateDefaultProfile(name: "default", brightness: 0f, contrast: 0f, saturation: 0f, sharpness: 0f, temperature: 0f, tint: 0f));
            SavedColorProfiles.Add(await CreateDefaultProfile(name: "brightness", brightness: 0.8f, contrast: 0f, saturation: 0f, sharpness: 0f, temperature: 0f, tint: 0f));
            SavedColorProfiles.Add(await CreateDefaultProfile(name: "contrast", brightness: 0.0f, contrast: 0.8f, saturation: 0f, sharpness: 0f, temperature: 0f, tint: 0f));
            SavedColorProfiles.Add(await CreateDefaultProfile(name: "warm", brightness: 0.0f, contrast: 0.0f, saturation: 0f, sharpness: 0f, temperature: 0.5f, tint: 0f));
            SavedColorProfiles.Add(await CreateDefaultProfile(name: "cold", brightness: 0.0f, contrast: 0.0f, saturation: 0f, sharpness: 0f, temperature: -0.5f, tint: 0f));
        }

        private async Task<SavedColorProfile> CreateDefaultProfile(string name, float brightness, float contrast, float saturation, float sharpness, float temperature, float tint)
        {
            var defaultProfile = new List<VideoEffectSliderProperty>
            {
                new VideoEffectSliderProperty(effectPropertyName: "Brightness", resolution: 0.01f, effectPropertyMaximum: 2.0f, effectPropertyValue: brightness, effectPropertyMinimum: -2.0f, defaultValue: 0.0f),
                new VideoEffectSliderProperty(effectPropertyName: "Contrast", resolution: 0.01f, effectPropertyMaximum: 2.0f, effectPropertyValue: contrast, effectPropertyMinimum: -2.0f, defaultValue: 0.0f),
                new VideoEffectSliderProperty(effectPropertyName: "Saturation", resolution: 0.01, effectPropertyMaximum: 2.0f, effectPropertyValue: saturation, effectPropertyMinimum: -2.0f, defaultValue: 0.0f),

                new VideoEffectSliderProperty(effectPropertyName: "Sharpness", resolution: 0.01f, effectPropertyMaximum: 10.0f, effectPropertyValue: sharpness, effectPropertyMinimum: 0f, defaultValue: 0.0f),
                new VideoEffectSliderProperty(effectPropertyName: "Temperature", resolution: 0.01f, effectPropertyMaximum: 1.0f, effectPropertyValue: temperature, effectPropertyMinimum: -1.0f, defaultValue: 0.0f),
                new VideoEffectSliderProperty(effectPropertyName: "Tint", resolution: 0.01f, effectPropertyMaximum: 1.0f, effectPropertyValue: tint, effectPropertyMinimum: -1.0f, defaultValue: 0.0f)
            };

            return await SaveColorProfileToFile(name, defaultProfile);
        }

        private async Task SaveColorProfileAsync(object arg)
        {
            try
            {
                StringInputDialog diag = new StringInputDialog("Save color profile", "Input name bellow");
                diag.Validator = StringInputDialog.FileNameValidator;
                var values = EffectProperties.ToList().AsEnumerable();
                await ContentDialogService.Instance.ShowAsync(diag);
                if (!string.IsNullOrWhiteSpace(diag.Result))
                {
                    var profile = await SaveColorProfileToFile(diag.Result, values);
                    SavedColorProfiles.Add(profile);
                }
            }
            catch { }
        }

        private async Task<SavedColorProfile> SaveColorProfileToFile(string path, IEnumerable<VideoEffectSliderProperty> values)
        {
            var saveFolder = await LocalCache.LocalFolders.GetVideoColorProfilesFolder();
            var saveFile = await saveFolder.CreateFileAsync(path, Windows.Storage.CreationCollisionOption.ReplaceExisting);

            var dictionaryValues = values.ToDictionary(x => x.EffectPropertyName, x => (object)x.EffectPropertyValue);

            var resultJson = JsonConvert.SerializeObject(dictionaryValues);
            await FileIO.WriteTextAsync(saveFile, resultJson);
            var profile = new SavedColorProfile(path);
            return profile;

        }

        private void ResetDefaultValues()
        {
            foreach (var obs in EffectProperties)
            {
                obs.EffectPropertyValue = obs.DefaultValue;
            }
        }

        private void Obs_ValueChanged(object? sender, float e)
        {
            switch ((sender as VideoEffectSliderProperty).EffectPropertyName)
            {
                case "Brightness": EffectConfig.Brightness = e; break;
                case "Contrast": EffectConfig.Contrast = e; break;
                case "Saturation": EffectConfig.Saturation = e; break;
                case "Sharpness": EffectConfig.Sharpness = e; break;
                case "Temperature": EffectConfig.Temperature = e; break;
                case "Tint": EffectConfig.Tint = e; break;
            }
            ProfileSliderValueChanged?.Invoke(this, sender);
        }
    }
}
