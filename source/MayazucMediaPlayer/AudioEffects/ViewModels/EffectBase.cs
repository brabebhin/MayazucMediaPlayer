using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.Services;
using System;
using Windows.Storage;

namespace MayazucMediaPlayer.AudioEffects.ViewModels
{
    public partial class AudioEffect : ObservableObject
    {
        public string Type
        {
            get;
            private set;
        }

        private bool _enabled;
        /// <summary>
        /// a user defined string used to identify this effect. Must be unique
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        public string DisplayTitle
        {
            get;
            set;
        }

        /// <summary>
        /// true if effect is to cause any changes to audio pipeline
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled == value) return;

                _enabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }


        public AudioEffect(string title, string type)
        {
            Type = type;
            Title = title;
            IsEnabled = GetStoreContainer().Values.ContainsKey(Title);
        }

        public void DisableEffect()
        {
            var container = GetStoreContainer();
            container.Values.Remove(Title);
            IsEnabled = false;
        }

        private ApplicationDataContainer GetStoreContainer()
        {
            switch (Type)
            {
                case EffectTypes.aecho:
                    return ApplicationDataContainers.EchoEffectsContainer;

                case EffectTypes.extraStereo:
                    return ApplicationDataContainers.ExtraStereoEffectsContainer;
                default: break;
            }
            //TODO: move on from application data containers
            return null;
        }

        public void EnableEffect()
        {
            var container = GetStoreContainer();
            if (!container.Values.ContainsKey(Title))
            {
                container.Values.Add(Title, GetSlimConfigurationString());
            }
        }

        public Func<string> GetSlimConfigurationString
        {
            get;
            set;
        }
    }
}
