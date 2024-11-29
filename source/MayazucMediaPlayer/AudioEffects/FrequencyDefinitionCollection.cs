using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.AudioEffects
{
    public partial class FrequencyDefinitionCollection : ObservableCollection<FrequencyDefinition>
    {
        public void AddRange(IEnumerable<FrequencyDefinition> definitions)
        {
            foreach (var d in definitions)
                Items.Add(d);

            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, definitions));
        }

        public FrequencyDefinitionCollection() : base()
        {
            Construct();
        }

        public FrequencyDefinitionCollection(FrequencyDefinitionCollection list) : base(list)
        {
            Construct();
        }

        internal ReadOnlyCollection<FrequencyDefinition> AsReadOnly()
        {
            return new ReadOnlyCollection<FrequencyDefinition>(Items);
        }

        public FrequencyDefinitionCollection(IEnumerable<FrequencyDefinition> collection) : base(collection)
        {
            Construct();
        }

        private void Construct()
        {
            SetOctaves();
            CollectionChanged += FrequencyDefinitionCollection_CollectionChanged;
        }

        private void FrequencyDefinitionCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetOctaves();
        }

        private void SetOctaves()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                double octave = 2;
                if (i < Items.Count - 1)
                {
                    octave = AudioEffectsExtensions.GetOctaves(Items[i].CutOffFrequency, Items[i + 1].CutOffFrequency);
                }
                Items[i].Octave = octave;
            }
        }
    }
}
