using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb;

public enum TmdbLanguage
{
    [FieldOption(Label = "Any Language", Hint = "any")]
    Any,

    [FieldOption(Label = "Abkhazian", Hint = "ab")]
    Ab,

    [FieldOption(Label = "Afar", Hint = "aa")]
    Aa,

    [FieldOption(Label = "Afrikaans", Hint = "af")]
    Af,

    [FieldOption(Label = "Akan", Hint = "ak")]
    Ak,

    [FieldOption(Label = "Albanian", Hint = "sq")]
    Sq,

    [FieldOption(Label = "Amharic", Hint = "am")]
    Am,

    [FieldOption(Label = "Arabic", Hint = "ar")]
    Ar,

    [FieldOption(Label = "Aragonese", Hint = "an")]
    An,

    [FieldOption(Label = "Armenian", Hint = "hy")]
    Hy,

    [FieldOption(Label = "Assamese", Hint = "as")]
    As,

    [FieldOption(Label = "Avaric", Hint = "av")]
    Av,

    [FieldOption(Label = "Avestan", Hint = "ae")]
    Ae,

    [FieldOption(Label = "Aymara", Hint = "ay")]
    Ay,

    [FieldOption(Label = "Azerbaijani", Hint = "az")]
    Az,

    [FieldOption(Label = "Bambara", Hint = "bm")]
    Bm,

    [FieldOption(Label = "Bashkir", Hint = "ba")]
    Ba,

    [FieldOption(Label = "Basque", Hint = "eu")]
    Eu,

    [FieldOption(Label = "Belarusian", Hint = "be")]
    Be,

    [FieldOption(Label = "Bengali", Hint = "bn")]
    Bn,

    [FieldOption(Label = "Bislama", Hint = "bi")]
    Bi,

    [FieldOption(Label = "Bosnian", Hint = "bs")]
    Bs,

    [FieldOption(Label = "Breton", Hint = "br")]
    Br,

    [FieldOption(Label = "Bulgarian", Hint = "bg")]
    Bg,

    [FieldOption(Label = "Burmese", Hint = "my")]
    My,

    [FieldOption(Label = "Cantonese", Hint = "cn")]
    Cn,

    [FieldOption(Label = "Catalan", Hint = "ca")]
    Ca,

    [FieldOption(Label = "Chamorro", Hint = "ch")]
    Ch,

    [FieldOption(Label = "Chechen", Hint = "ce")]
    Ce,

    [FieldOption(Label = "Chichewa; Nyanja", Hint = "ny")]
    Ny,

    [FieldOption(Label = "Chuvash", Hint = "cv")]
    Cv,

    [FieldOption(Label = "Cornish", Hint = "kw")]
    Kw,

    [FieldOption(Label = "Corsican", Hint = "co")]
    Co,

    [FieldOption(Label = "Cree", Hint = "cr")]
    Cr,

    [FieldOption(Label = "Croatian", Hint = "hr")]
    Hr,

    [FieldOption(Label = "Czech", Hint = "cs")]
    Cs,

    [FieldOption(Label = "Danish", Hint = "da")]
    Da,

    [FieldOption(Label = "Divehi", Hint = "dv")]
    Dv,

    [FieldOption(Label = "Dutch", Hint = "nl")]
    Nl,

    [FieldOption(Label = "Dzongkha", Hint = "dz")]
    Dz,

    [FieldOption(Label = "English", Hint = "en")]
    En,

    [FieldOption(Label = "Esperanto", Hint = "eo")]
    Eo,

    [FieldOption(Label = "Estonian", Hint = "et")]
    Et,

    [FieldOption(Label = "Ewe", Hint = "ee")]
    Ee,

    [FieldOption(Label = "Faroese", Hint = "fo")]
    Fo,

    [FieldOption(Label = "Fijian", Hint = "fj")]
    Fj,

    [FieldOption(Label = "Finnish", Hint = "fi")]
    Fi,

    [FieldOption(Label = "French", Hint = "fr")]
    Fr,

    [FieldOption(Label = "Frisian", Hint = "fy")]
    Fy,

    [FieldOption(Label = "Fulah", Hint = "ff")]
    Ff,

    [FieldOption(Label = "Gaelic", Hint = "gd")]
    Gd,

    [FieldOption(Label = "Galician", Hint = "gl")]
    Gl,

    [FieldOption(Label = "Ganda", Hint = "lg")]
    Lg,

    [FieldOption(Label = "Georgian", Hint = "ka")]
    Ka,

    [FieldOption(Label = "German", Hint = "de")]
    De,

    [FieldOption(Label = "Greek", Hint = "el")]
    El,

    [FieldOption(Label = "Guarani", Hint = "gn")]
    Gn,

    [FieldOption(Label = "Gujarati", Hint = "gu")]
    Gu,

    [FieldOption(Label = "Haitian; Haitian Creole", Hint = "ht")]
    Ht,

    [FieldOption(Label = "Hausa", Hint = "ha")]
    Ha,

    [FieldOption(Label = "Hebrew", Hint = "he")]
    He,

    [FieldOption(Label = "Herero", Hint = "hz")]
    Hz,

    [FieldOption(Label = "Hindi", Hint = "hi")]
    Hi,

    [FieldOption(Label = "Hiri Motu", Hint = "ho")]
    Ho,

    [FieldOption(Label = "Hungarian", Hint = "hu")]
    Hu,

    [FieldOption(Label = "Icelandic", Hint = "is")]
    Is,

    [FieldOption(Label = "Ido", Hint = "io")]
    Io,

    [FieldOption(Label = "Igbo", Hint = "ig")]
    Ig,

    [FieldOption(Label = "Indonesian", Hint = "id")]
    Id,

    [FieldOption(Label = "Interlingua", Hint = "ia")]
    Ia,

    [FieldOption(Label = "Interlingue", Hint = "ie")]
    Ie,

    [FieldOption(Label = "Inuktitut", Hint = "iu")]
    Iu,

    [FieldOption(Label = "Inupiaq", Hint = "ik")]
    Ik,

    [FieldOption(Label = "Irish", Hint = "ga")]
    Ga,

    [FieldOption(Label = "Italian", Hint = "it")]
    It,

    [FieldOption(Label = "Japanese", Hint = "ja")]
    Ja,

    [FieldOption(Label = "Javanese", Hint = "jv")]
    Jv,

    [FieldOption(Label = "Kalaallisut", Hint = "kl")]
    Kl,

    [FieldOption(Label = "Kannada", Hint = "kn")]
    Kn,

    [FieldOption(Label = "Kanuri", Hint = "kr")]
    Kr,

    [FieldOption(Label = "Kashmiri", Hint = "ks")]
    Ks,

    [FieldOption(Label = "Kazakh", Hint = "kk")]
    Kk,

    [FieldOption(Label = "Khmer", Hint = "km")]
    Km,

    [FieldOption(Label = "Kikuyu", Hint = "ki")]
    Ki,

    [FieldOption(Label = "Kinyarwanda", Hint = "rw")]
    Rw,

    [FieldOption(Label = "Kirghiz", Hint = "ky")]
    Ky,

    [FieldOption(Label = "Komi", Hint = "kv")]
    Kv,

    [FieldOption(Label = "Kongo", Hint = "kg")]
    Kg,

    [FieldOption(Label = "Korean", Hint = "ko")]
    Ko,

    [FieldOption(Label = "Kuanyama", Hint = "kj")]
    Kj,

    [FieldOption(Label = "Kurdish", Hint = "ku")]
    Ku,

    [FieldOption(Label = "Lao", Hint = "lo")]
    Lo,

    [FieldOption(Label = "Latin", Hint = "la")]
    La,

    [FieldOption(Label = "Latvian", Hint = "lv")]
    Lv,

    [FieldOption(Label = "Letzeburgesch", Hint = "lb")]
    Lb,

    [FieldOption(Label = "Limburgish", Hint = "li")]
    Li,

    [FieldOption(Label = "Lingala", Hint = "ln")]
    Ln,

    [FieldOption(Label = "Lithuanian", Hint = "lt")]
    Lt,

    [FieldOption(Label = "Luba-Katanga", Hint = "lu")]
    Lu,

    [FieldOption(Label = "Macedonian", Hint = "mk")]
    Mk,

    [FieldOption(Label = "Malagasy", Hint = "mg")]
    Mg,

    [FieldOption(Label = "Malay", Hint = "ms")]
    Ms,

    [FieldOption(Label = "Malayalam", Hint = "ml")]
    Ml,

    [FieldOption(Label = "Maltese", Hint = "mt")]
    Mt,

    [FieldOption(Label = "Mandarin", Hint = "zh")]
    Zh,

    [FieldOption(Label = "Manx", Hint = "gv")]
    Gv,

    [FieldOption(Label = "Maori", Hint = "mi")]
    Mi,

    [FieldOption(Label = "Marathi", Hint = "mr")]
    Mr,

    [FieldOption(Label = "Marshall", Hint = "mh")]
    Mh,

    [FieldOption(Label = "Moldavian", Hint = "mo")]
    Mo,

    [FieldOption(Label = "Mongolian", Hint = "mn")]
    Mn,

    [FieldOption(Label = "Nauru", Hint = "na")]
    Na,

    [FieldOption(Label = "Navajo", Hint = "nv")]
    Nv,

    [FieldOption(Label = "Ndebele", Hint = "nr")]
    Nr,

    [FieldOption(Label = "Ndebele", Hint = "nd")]
    Nd,

    [FieldOption(Label = "Ndonga", Hint = "ng")]
    Ng,

    [FieldOption(Label = "Nepali", Hint = "ne")]
    Ne,

    [FieldOption(Label = "No Language", Hint = "xx")]
    Xx,

    [FieldOption(Label = "Northern Sami", Hint = "se")]
    Se,

    [FieldOption(Label = "Norwegian", Hint = "no")]
    No,

    [FieldOption(Label = "Norwegian Bokmål", Hint = "nb")]
    Nb,

    [FieldOption(Label = "Norwegian Nynorsk", Hint = "nn")]
    Nn,

    [FieldOption(Label = "Occitan", Hint = "oc")]
    Oc,

    [FieldOption(Label = "Ojibwa", Hint = "oj")]
    Oj,

    [FieldOption(Label = "Oriya", Hint = "or")]
    Or,

    [FieldOption(Label = "Oromo", Hint = "om")]
    Om,

    [FieldOption(Label = "Ossetian; Ossetic", Hint = "os")]
    Os,

    [FieldOption(Label = "Pali", Hint = "pi")]
    Pi,

    [FieldOption(Label = "Persian", Hint = "fa")]
    Fa,

    [FieldOption(Label = "Polish", Hint = "pl")]
    Pl,

    [FieldOption(Label = "Portuguese", Hint = "pt")]
    Pt,

    [FieldOption(Label = "Punjabi", Hint = "pa")]
    Pa,

    [FieldOption(Label = "Pushto", Hint = "ps")]
    Ps,

    [FieldOption(Label = "Quechua", Hint = "qu")]
    Qu,

    [FieldOption(Label = "Raeto-Romance", Hint = "rm")]
    Rm,

    [FieldOption(Label = "Romanian", Hint = "ro")]
    Ro,

    [FieldOption(Label = "Rundi", Hint = "rn")]
    Rn,

    [FieldOption(Label = "Russian", Hint = "ru")]
    Ru,

    [FieldOption(Label = "Samoan", Hint = "sm")]
    Sm,

    [FieldOption(Label = "Sango", Hint = "sg")]
    Sg,

    [FieldOption(Label = "Sanskrit", Hint = "sa")]
    Sa,

    [FieldOption(Label = "Sardinian", Hint = "sc")]
    Sc,

    [FieldOption(Label = "Serbian", Hint = "sr")]
    Sr,

    [FieldOption(Label = "Serbo-Croatian", Hint = "sh")]
    Sh,

    [FieldOption(Label = "Shona", Hint = "sn")]
    Sn,

    [FieldOption(Label = "Sindhi", Hint = "sd")]
    Sd,

    [FieldOption(Label = "Sinhalese", Hint = "si")]
    Si,

    [FieldOption(Label = "Slavic", Hint = "cu")]
    Cu,

    [FieldOption(Label = "Slovak", Hint = "sk")]
    Sk,

    [FieldOption(Label = "Slovenian", Hint = "sl")]
    Sl,

    [FieldOption(Label = "Somali", Hint = "so")]
    So,

    [FieldOption(Label = "Sotho", Hint = "st")]
    St,

    [FieldOption(Label = "Spanish", Hint = "es")]
    Es,

    [FieldOption(Label = "Sundanese", Hint = "su")]
    Su,

    [FieldOption(Label = "Swahili", Hint = "sw")]
    Sw,

    [FieldOption(Label = "Swati", Hint = "ss")]
    Ss,

    [FieldOption(Label = "Swedish", Hint = "sv")]
    Sv,

    [FieldOption(Label = "Tagalog", Hint = "tl")]
    Tl,

    [FieldOption(Label = "Tahitian", Hint = "ty")]
    Ty,

    [FieldOption(Label = "Tajik", Hint = "tg")]
    Tg,

    [FieldOption(Label = "Tamil", Hint = "ta")]
    Ta,

    [FieldOption(Label = "Tatar", Hint = "tt")]
    Tt,

    [FieldOption(Label = "Telugu", Hint = "te")]
    Te,

    [FieldOption(Label = "Thai", Hint = "th")]
    Th,

    [FieldOption(Label = "Tibetan", Hint = "bo")]
    Bo,

    [FieldOption(Label = "Tigrinya", Hint = "ti")]
    Ti,

    [FieldOption(Label = "Tonga", Hint = "to")]
    To,

    [FieldOption(Label = "Tsonga", Hint = "ts")]
    Ts,

    [FieldOption(Label = "Tswana", Hint = "tn")]
    Tn,

    [FieldOption(Label = "Turkish", Hint = "tr")]
    Tr,

    [FieldOption(Label = "Turkmen", Hint = "tk")]
    Tk,

    [FieldOption(Label = "Twi", Hint = "tw")]
    Tw,

    [FieldOption(Label = "Uighur", Hint = "ug")]
    Ug,

    [FieldOption(Label = "Ukrainian", Hint = "uk")]
    Uk,

    [FieldOption(Label = "Urdu", Hint = "ur")]
    Ur,

    [FieldOption(Label = "Uzbek", Hint = "uz")]
    Uz,

    [FieldOption(Label = "Venda", Hint = "ve")]
    Ve,

    [FieldOption(Label = "Vietnamese", Hint = "vi")]
    Vi,

    [FieldOption(Label = "Volapük", Hint = "vo")]
    Vo,

    [FieldOption(Label = "Walloon", Hint = "wa")]
    Wa,

    [FieldOption(Label = "Welsh", Hint = "cy")]
    Cy,

    [FieldOption(Label = "Wolof", Hint = "wo")]
    Wo,

    [FieldOption(Label = "Xhosa", Hint = "xh")]
    Xh,

    [FieldOption(Label = "Yi", Hint = "ii")]
    Ii,

    [FieldOption(Label = "Yiddish", Hint = "yi")]
    Yi,

    [FieldOption(Label = "Yoruba", Hint = "yo")]
    Yo,

    [FieldOption(Label = "Zhuang", Hint = "za")]
    Za,

    [FieldOption(Label = "Zulu", Hint = "zu")]
    Zu
}
