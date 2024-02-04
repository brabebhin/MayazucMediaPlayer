using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.AudioEffects
{
    public class AudioEqualizerPresetCollection : ObservableCollection<AudioEqualizerPreset>
    {
        public AudioEqualizerPresetCollection()
        {
        }

        public AudioEqualizerPresetCollection(List<AudioEqualizerPreset> list) : base(list)
        {
        }

        public AudioEqualizerPresetCollection(IEnumerable<AudioEqualizerPreset> collection) : base(collection)
        {
        }

        public void AddRange(IEnumerable<AudioEqualizerPreset> presets)
        {
            foreach (var v in presets)
                base.Items.Add(v);

            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        public new void Add(AudioEqualizerPreset preset)
        {
            base.Add(preset);
        }

        public new void Remove(AudioEqualizerPreset preset)
        {
            base.Remove(preset);
        }
    }
}
