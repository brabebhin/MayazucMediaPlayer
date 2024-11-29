using MayazucMediaPlayer.Services;
using System;

namespace MayazucMediaPlayer.VideoEffects
{
    public sealed partial class SavedColorProfile : ObservableObject
    {
        public string Name
        {
            get;
            private set;
        }

        bool? defualtChecked;
        public bool IsDefault
        {
            get
            {
                if (!defualtChecked.HasValue)
                {
                    defualtChecked = Name == "default";
                }

                return defualtChecked.Value;
            }
        }

        public SavedColorProfile(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            return obj is SavedColorProfile profile &&
                   Name == profile.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
