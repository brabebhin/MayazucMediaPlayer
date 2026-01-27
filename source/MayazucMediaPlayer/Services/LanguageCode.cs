using System;
using System.Collections.Generic;

namespace MayazucMediaPlayer.Services
{
    public class LanguageCode
    {
        public string TwoLetterIsoCode
        {
            get;
            private set;
        }

        public string ThreeLetterIsoCode
        {
            get;
            private set;
        }

        public string LanguageName
        {
            get;
            private set;
        }

        public LanguageCode(string threeCode, string code, string name)
        {
            ThreeLetterIsoCode = threeCode;
            TwoLetterIsoCode = code;
            LanguageName = name;
        }

        public override string ToString()
        {
            return $"{LanguageName} - ({ThreeLetterIsoCode})";
        }

        public override bool Equals(object obj)
        {
            var code = obj as LanguageCode;
            if (code != null)
            {
                return code != null &&
                       TwoLetterIsoCode == code.TwoLetterIsoCode &&
                       ThreeLetterIsoCode == code.ThreeLetterIsoCode &&
                       LanguageName == code.LanguageName;
            }
            else
            {
                var str = obj as string;
                if (str != null)
                {
                    return TwoLetterIsoCode.Equals(str, StringComparison.OrdinalIgnoreCase) || ThreeLetterIsoCode.Equals(str, StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 32262704;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TwoLetterIsoCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ThreeLetterIsoCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LanguageName);
            return hashCode;
        }
    }
}
