﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MayazucMediaPlayer.Services
{
    public static class LanguageCodesService
    {
        private static readonly List<LanguageCode> _codes;

        static LanguageCodesService()
        {
            _codes = new List<LanguageCode>()
            {
                //new LanguageCode("alpha2","English"),
                new LanguageCode("aar","aa","Afar"),
                new LanguageCode("abk","ab","Abkhazian"),
                new LanguageCode("afr","af","Afrikaans"),
                new LanguageCode("aka","ak","Akan"),
                new LanguageCode("alb","sq","Albanian"),
                new LanguageCode("amh","am","Amharic"),
                new LanguageCode("ara","ar","Arabic"),
                new LanguageCode("arg","an","Aragonese"),
                new LanguageCode("arm","hy","Armenian"),
                new LanguageCode("asm","as","Assamese"),
                new LanguageCode("ava","av","Avaric"),
                new LanguageCode("ave","ae","Avestan"),
                new LanguageCode("aym","ay","Aymara"),
                new LanguageCode("aze","az","Azerbaijani"),
                new LanguageCode("bak","ba","Bashkir"),
                new LanguageCode("bam","bm","Bambara"),
                new LanguageCode("baq","eu","Basque"),
                new LanguageCode("bel","be","Belarusian"),
                new LanguageCode("ben","bn","Bengali"),
                new LanguageCode("bih","bh","Bihari languages"),
                new LanguageCode("bis","bi","Bislama"),
                new LanguageCode("bos","bs","Bosnian"),
                new LanguageCode("bre","br","Breton"),
                new LanguageCode("bul","bg","Bulgarian"),
                new LanguageCode("bur","my","Burmese"),
                new LanguageCode("cat","ca","Catalan; Valencian"),
                new LanguageCode("cha","ch","Chamorro"),
                new LanguageCode("che","ce","Chechen"),
                new LanguageCode("chi","zh","Chinese"),
                new LanguageCode("chu","cu","Church Slavic; Old Slavonic; Church Slavonic; Old Bulgarian; Old Church Slavonic"),
                new LanguageCode("chv","cv","Chuvash"),
                new LanguageCode("cor","kw","Cornish"),
                new LanguageCode("cos","co","Corsican"),
                new LanguageCode("cre","cr","Cree"),
                new LanguageCode("cze","cs","Czech"),
                new LanguageCode("dan","da","Danish"),
                new LanguageCode("div","dv","Divehi; Dhivehi; Maldivian"),
                new LanguageCode("dut","nl","Dutch; Flemish"),
                new LanguageCode("dzo","dz","Dzongkha"),
                new LanguageCode("eng","en","English"),
                new LanguageCode("epo","eo","Esperanto"),
                new LanguageCode("est","et","Estonian"),
                new LanguageCode("ewe","ee","Ewe"),
                new LanguageCode("fao","fo","Faroese"),
                new LanguageCode("fij","fj","Fijian"),
                new LanguageCode("fin","fi","Finnish"),
                new LanguageCode("fre","fr","French"),
                new LanguageCode("fry","fy","Western Frisian"),
                new LanguageCode("ful","ff","Fulah"),
                new LanguageCode("geo","ka","Georgian"),
                new LanguageCode("ger","de","German"),
                new LanguageCode("gla","gd","Gaelic; Scottish Gaelic"),
                new LanguageCode("gle","ga","Irish"),
                new LanguageCode("glg","gl","Galician"),
                new LanguageCode("glv","gv","Manx"),
                new LanguageCode("gre","el","Greek  Modern (1453-)"),
                new LanguageCode("grn","gn","Guarani"),
                new LanguageCode("guj","gu","Gujarati"),
                new LanguageCode("hat","ht","Haitian; Haitian Creole"),
                new LanguageCode("hau","ha","Hausa"),
                new LanguageCode("heb","he","Hebrew"),
                new LanguageCode("her","hz","Herero"),
                new LanguageCode("hin","hi","Hindi"),
                new LanguageCode("hmo","ho","Hiri Motu"),
                new LanguageCode("hrv","hr","Croatian"),
                new LanguageCode("hun","hu","Hungarian"),
                new LanguageCode("ibo","ig","Igbo"),
                new LanguageCode("ice","is","Icelandic"),
                new LanguageCode("ido","io","Ido"),
                new LanguageCode("iii","ii","Sichuan Yi; Nuosu"),
                new LanguageCode("iku","iu","Inuktitut"),
                new LanguageCode("ile","ie","Interlingue; Occidental"),
                new LanguageCode("ina","ia","Interlingua (International Auxiliary Language Association)"),
                new LanguageCode("ind","id","Indonesian"),
                new LanguageCode("ipk","ik","Inupiaq"),
                new LanguageCode("ita","it","Italian"),
                new LanguageCode("jav","jv","Javanese"),
                new LanguageCode("jpn","ja","Japanese"),
                new LanguageCode("kal","kl","Kalaallisut; Greenlandic"),
                new LanguageCode("kan","kn","Kannada"),
                new LanguageCode("kas","ks","Kashmiri"),
                new LanguageCode("kau","kr","Kanuri"),
                new LanguageCode("kaz","kk","Kazakh"),
                new LanguageCode("khm","km","Central Khmer"),
                new LanguageCode("kik","ki","Kikuyu; Gikuyu"),
                new LanguageCode("kin","rw","Kinyarwanda"),
                new LanguageCode("kir","ky","Kirghiz; Kyrgyz"),
                new LanguageCode("kom","kv","Komi"),
                new LanguageCode("kon","kg","Kongo"),
                new LanguageCode("kor","ko","Korean"),
                new LanguageCode("kua","kj","Kuanyama; Kwanyama"),
                new LanguageCode("kur","ku","Kurdish"),
                new LanguageCode("lao","lo","Lao"),
                new LanguageCode("lat","la","Latin"),
                new LanguageCode("lav","lv","Latvian"),
                new LanguageCode("lim","li","Limburgan; Limburger; Limburgish"),
                new LanguageCode("lin","ln","Lingala"),
                new LanguageCode("lit","lt","Lithuanian"),
                new LanguageCode("ltz","lb","Luxembourgish; Letzeburgesch"),
                new LanguageCode("lub","lu","Luba-Katanga"),
                new LanguageCode("lug","lg","Ganda"),
                new LanguageCode("mac","mk","Macedonian"),
                new LanguageCode("mah","mh","Marshallese"),
                new LanguageCode("mal","ml","Malayalam"),
                new LanguageCode("mao","mi","Maori"),
                new LanguageCode("mar","mr","Marathi"),
                new LanguageCode("may","ms","Malay"),
                new LanguageCode("mlg","mg","Malagasy"),
                new LanguageCode("mlt","mt","Maltese"),
                new LanguageCode("mon","mn","Mongolian"),
                new LanguageCode("nau","na","Nauru"),
                new LanguageCode("nav","nv","Navajo; Navaho"),
                new LanguageCode("nbl","nr","Ndebele South; South Ndebele"),
                new LanguageCode("nde", "nd", "Ndebele North; North Ndebele"),
                new LanguageCode("ndo", "ng", "Ndonga"),
                new LanguageCode("nep", "ne", "Nepali"),
                new LanguageCode("nno", "nn", "Norwegian Nynorsk; Nynorsk  Norwegian"),
                new LanguageCode("nob", "nb", "BokmÃ¥l  Norwegian; Norwegian BokmÃ¥l"),
                new LanguageCode("nor", "no", "Norwegian"),
                new LanguageCode("nya", "ny", "Chichewa; Chewa; Nyanja"),
                new LanguageCode("oci", "oc", "Occitan (post 1500); ProvenÃ§al"),
                new LanguageCode("oji", "oj", "Ojibwa"),
                new LanguageCode("ori", "or", "Oriya"),
                new LanguageCode("orm", "om", "Oromo"),
                new LanguageCode("oss", "os", "Ossetian; Ossetic"),
                new LanguageCode("pan", "pa", "Panjabi; Punjabi"),
                new LanguageCode("per", "fa", "Persian"),
                new LanguageCode("pli", "pi", "Pali"),
                new LanguageCode("pol", "pl", "Polish"),
                new LanguageCode("por", "pt", "Portuguese"),
                new LanguageCode("pus", "ps", "Pushto; Pashto"),
                new LanguageCode("que", "qu", "Quechua"),
                new LanguageCode("roh", "rm", "Romansh"),
                new LanguageCode("rum", "ro", "Romanian; Moldavian; Moldovan"),
                new LanguageCode("run", "rn", "Rundi"),
                new LanguageCode("rus", "ru", "Russian"),
                new LanguageCode("sag", "sg", "Sango"),
                new LanguageCode("san", "sa", "Sanskrit"),
                new LanguageCode("sin", "si", "Sinhala; Sinhalese"),
                new LanguageCode("slo", "sk", "Slovak"),
                new LanguageCode("slv", "sl", "Slovenian"),
                new LanguageCode("sme", "se", "Northern Sami"),
                new LanguageCode("smo", "sm", "Samoan"),
                new LanguageCode("sna", "sn", "Shona"),
                new LanguageCode("snd", "sd", "Sindhi"),
                new LanguageCode("som", "so", "Somali"),
                new LanguageCode("sot", "st", "Sotho Southern"),
                new LanguageCode("spa", "es", "Spanish; Castilian"),
                new LanguageCode("srd", "sc", "Sardinian"),
                new LanguageCode("srp", "sr", "Serbian"),
                new LanguageCode("ssw", "ss", "Swati"),
                new LanguageCode("sun", "su", "Sundanese"),
                new LanguageCode("swa", "sw", "Swahili"),
                new LanguageCode("swe", "sv", "Swedish"),
                new LanguageCode("tah", "ty", "Tahitian"),
                new LanguageCode("tam", "ta", "Tamil"),
                new LanguageCode("tat", "tt", "Tatar"),
                new LanguageCode("tel", "te", "Telugu"),
                new LanguageCode("tgk", "tg", "Tajik"),
                new LanguageCode("tgl", "tl", "Tagalog"),
                new LanguageCode("tha", "th", "Thai"),
                new LanguageCode("tib", "bo", "Tibetan"),
                new LanguageCode("tir", "ti", "Tigrinya"),
                new LanguageCode("ton", "to", "Tonga (Tonga Islands)"),
                new LanguageCode("tsn", "tn", "Tswana"),
                new LanguageCode("tso", "ts", "Tsonga"),
                new LanguageCode("tuk", "tk", "Turkmen"),
                new LanguageCode("tur", "tr", "Turkish"),
                new LanguageCode("twi", "tw", "Twi"),
                new LanguageCode("uig", "ug", "Uighur; Uyghur"),
                new LanguageCode("ukr", "uk", "Ukrainian"),
                new LanguageCode("urd", "ur", "Urdu"),
                new LanguageCode("uzb", "uz", "Uzbek"),
                new LanguageCode("ven", "ve", "Venda"),
                new LanguageCode("vie", "vi", "Vietnamese"),
                new LanguageCode("vol", "vo", "VolapÃ¼k"),
                new LanguageCode("wel", "cy", "Welsh"),
                new LanguageCode("wln", "wa", "Walloon"),
                new LanguageCode("wol", "wo", "Wolof"),
                new LanguageCode("xho", "xh", "Xhosa"),
                new LanguageCode("yid", "yi", "Yiddish"),
                new LanguageCode("yor", "yo", "Yoruba"),
                new LanguageCode("zha", "za", "Zhuang; Chuang"),
                new LanguageCode("zul", "zu", "Zulu")
            }.OrderBy(x => x.LanguageName).ToList();
        }

        public static IList<LanguageCode> Codes
        {
            get
            {
                return _codes.AsReadOnly();
            }
        }

        public static int GetDefaultLanguageIndex()
        {
            var i = 43;
            var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            var matched = Codes.FirstOrDefault(x => x.TwoLetterIsoCode == currentCulture);
            if (matched != null)
            {
                return Codes.IndexOf(matched);
            }
            return i;
        }
    }


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
