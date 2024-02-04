using FluentResults;
using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.AudioEffects
{
    public class EqualizerConfiguration : ObservableObject
    {
        public const string DefaultConfigurationName = "MC Media Center";


        public event EventHandler Changed;

        public string Name
        {
            get;
            private set;
        }

        public string FrequencyDisplays
        {
            get;
            private set;
        }

        readonly FrequencyDefinitionCollection _bands = new FrequencyDefinitionCollection();

        public ReadOnlyCollection<FrequencyDefinition> Bands
        {
            get
            {
                return _bands.AsReadOnly();
            }
        }

        public AudioEqualizerPresetCollection Presets
        {
            get;
            set;
        } = new AudioEqualizerPresetCollection();

        public bool CanDelete
        {
            get
            {
                return DefaultConfigurationName != Name;
            }
        }

        public EqualizerConfiguration(IEnumerable<FrequencyDefinition> bands,
            IEnumerable<AudioEqualizerPreset> presets, string name)
        {
            _bands.AddRange(bands.OrderBy(x => x.CutOffFrequency));
            if (presets != null)
                Presets.AddRange(presets);
            Name = name;

            var defaultFreq = new int[Bands.Count];
            for (int i = 0; i < defaultFreq.Length; i++)
            {
                defaultFreq[i] = 0;
            }

            foreach (var v in bands)
            {
                FrequencyDisplays += v.CutOffFrequency + " ";
            }

            if (!Presets.Any(x => x.PresetName == "default"))
                Presets.Add(new AudioEqualizerPreset("default", "", defaultFreq));
        }


        /// <summary>
        /// returns the default equalizer configuration
        /// </summary>
        /// <returns></returns>
        public static EqualizerConfiguration GetDefault()
        {
            FrequencyDefinition def1 = new FrequencyDefinition(400);
            FrequencyDefinition def2 = new FrequencyDefinition(650);
            FrequencyDefinition def3 = new FrequencyDefinition(1000);
            FrequencyDefinition def4 = new FrequencyDefinition(1500);
            FrequencyDefinition def5 = new FrequencyDefinition(2200);
            FrequencyDefinition def6 = new FrequencyDefinition(3500);
            FrequencyDefinition def7 = new FrequencyDefinition(7000);

            FrequencyDefinitionCollection b = new FrequencyDefinitionCollection();
            b.Add(def1);
            b.Add(def2);
            b.Add(def3);
            b.Add(def4);
            b.Add(def5);
            b.Add(def6);
            b.Add(def7);

            List<AudioEqualizerPreset> p = new List<AudioEqualizerPreset>();

            p.Add(new AudioEqualizerPreset("Acoustic", "", new int[] { 4, 3, 0, 1, 2, 3, 2 }));
            p.Add(new AudioEqualizerPreset("French Pop", "", new int[] { 4, 3, 2, 1, 2, 3, 2 }));
            p.Add(new AudioEqualizerPreset("Bass boost", "", new int[] { 5, 3, 2, 1, 0, 0, 0 }));
            p.Add(new AudioEqualizerPreset("Classical", "Classical", new int[] { 4, 2, 0, -1, 0, 2, 3 }));
            p.Add(new AudioEqualizerPreset("Dance", "Dance", new int[] { 6, 5, 0, 2, 4, 4, 3 }));
            p.Add(new AudioEqualizerPreset("Electronic", "Electronic", new int[] { 3, 1, -2, -1, 0, 0, 3 }));
            p.Add(new AudioEqualizerPreset("Hip hop", "Hip hop", new int[] { 4, 1, 0, -1, 0, 0, 2 }));
            p.Add(new AudioEqualizerPreset("Jazz", "Jazz", new int[] { 2, 1, 1, 0, 1, 1, 2 }));
            p.Add(new AudioEqualizerPreset("Loudness", "", new int[] { 5, 0, -1, 0, -1, -2, 5 }));
            p.Add(new AudioEqualizerPreset("Pop", "Pop", new int[] { -1, -1, 3, 3, 2, 0, -1 }));
            p.Add(new AudioEqualizerPreset("Rock", "Rock", new int[] { 4, 2, 0, 0, 0, 2, 3 }));
            p.Add(new AudioEqualizerPreset("Speech", "Speech", new int[] { -1, 0, 1, 3, 4, 4, 2 }));
            p.Add(new AudioEqualizerPreset("Treble boost", "", new int[] { 0, 0, 0, 1, 2, 3, 5 }));
            p.Add(new AudioEqualizerPreset("Vocal boost", "", new int[] { 0, 0, 2, 5, 2, 0, 0 }));

            return new EqualizerConfiguration(b, p, "MC Media Center");
        }

        public void SetPresets(IEnumerable<AudioEqualizerPreset> p)
        {
            Presets.Clear();
            Presets.AddRange(p);
            NotifyPropertyChanged(nameof(Presets));
        }

        public void SetDefault()
        {
            SettingsWrapper.EqualizerConfiguration = Name;
        }

        public bool CompareTo(EqualizerConfiguration other, bool includePresets)
        {
            bool result = true;

            result = result & (Bands.Count == other.Bands.Count);
            if (!result) return result;

            for (int i = 0; i < Bands.Count; i++)
            {
                result = result & (Bands[i].Equals(other.Bands[i]));
                if (!result) return result;
            }
            if (includePresets)
            {
                result = result & (Presets.Count == other.Presets.Count);
                if (!result) return result;

                for (int i = 0; i < Presets.Count; i++)
                {
                    result = result & (Presets[i].Equals(other.Presets[i]));
                    if (!result) return result;
                }
            }

            return result;
        }

        public static async Task<Result<EqualizerConfiguration>> LoadFromFile(string name)
        {
            var saveFolder = Directory.CreateDirectory(await LocalFolders.GetEqualizerPresetsFolderAsync());
            var storedFile = saveFolder.EnumerateFiles().FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.Name) == name);
            return LoadFromFile(storedFile);
        }

        public static Result<EqualizerConfiguration> LoadFromFile(FileInfo storedFile)
        {
            var json = File.ReadAllText(storedFile.FullName);
            try
            {
                return Result.Ok(JsonConvert.DeserializeObject<EqualizerConfiguration>(json));
            }
            catch
            {

            }
            return Result.Fail("Could not load equalizer config from file");
        }

        public void SaveToFileAsync()
        {
            var saveFolder = Directory.CreateDirectory(LocalFolders.EqualizerConfigurationsPayloadFolderPath());
            var filePath = Path.Combine(saveFolder.FullName, Name + ".json");
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(filePath, json);
            Changed?.Invoke(this, new EventArgs());
        }

        public void Delete()
        {
            var saveFolder = Directory.CreateDirectory(LocalFolders.EqualizerConfigurationsPayloadFolderPath());
            var filePath = Path.Combine(saveFolder.FullName, Name + ".json");
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
                fileInfo.Delete();
        }
    }
}