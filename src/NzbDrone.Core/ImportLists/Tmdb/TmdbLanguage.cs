using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb;

// The numeric values are persisted in import list settings. Therefore, never reorder or reuse them
// but assign the next unused number when adding a language (even if inserted alphabetically).
// The member names are sent to TMDb as the lowercase ISO 639-1 code, so don't rename them.
public enum TmdbLanguage
{
    [FieldOption(Label = "Any Language", Hint = "any")]
    Any = 0,

    [FieldOption(Label = "Abkhazian", Hint = "ab")]
    Ab = 1,

    [FieldOption(Label = "Afar", Hint = "aa")]
    Aa = 2,

    [FieldOption(Label = "Afrikaans", Hint = "af")]
    Af = 3,

    [FieldOption(Label = "Akan", Hint = "ak")]
    Ak = 4,

    [FieldOption(Label = "Albanian", Hint = "sq")]
    Sq = 5,

    [FieldOption(Label = "Amharic", Hint = "am")]
    Am = 6,

    [FieldOption(Label = "Arabic", Hint = "ar")]
    Ar = 7,

    [FieldOption(Label = "Aragonese", Hint = "an")]
    An = 8,

    [FieldOption(Label = "Armenian", Hint = "hy")]
    Hy = 9,

    [FieldOption(Label = "Assamese", Hint = "as")]
    As = 10,

    [FieldOption(Label = "Avaric", Hint = "av")]
    Av = 11,

    [FieldOption(Label = "Avestan", Hint = "ae")]
    Ae = 12,

    [FieldOption(Label = "Aymara", Hint = "ay")]
    Ay = 13,

    [FieldOption(Label = "Azerbaijani", Hint = "az")]
    Az = 14,

    [FieldOption(Label = "Bambara", Hint = "bm")]
    Bm = 15,

    [FieldOption(Label = "Bashkir", Hint = "ba")]
    Ba = 16,

    [FieldOption(Label = "Basque", Hint = "eu")]
    Eu = 17,

    [FieldOption(Label = "Belarusian", Hint = "be")]
    Be = 18,

    [FieldOption(Label = "Bengali", Hint = "bn")]
    Bn = 19,

    [FieldOption(Label = "Bislama", Hint = "bi")]
    Bi = 20,

    [FieldOption(Label = "Bosnian", Hint = "bs")]
    Bs = 21,

    [FieldOption(Label = "Breton", Hint = "br")]
    Br = 22,

    [FieldOption(Label = "Bulgarian", Hint = "bg")]
    Bg = 23,

    [FieldOption(Label = "Burmese", Hint = "my")]
    My = 24,

    [FieldOption(Label = "Cantonese", Hint = "cn")]
    Cn = 25,

    [FieldOption(Label = "Catalan", Hint = "ca")]
    Ca = 26,

    [FieldOption(Label = "Chamorro", Hint = "ch")]
    Ch = 27,

    [FieldOption(Label = "Chechen", Hint = "ce")]
    Ce = 28,

    [FieldOption(Label = "Chichewa; Nyanja", Hint = "ny")]
    Ny = 29,

    [FieldOption(Label = "Chuvash", Hint = "cv")]
    Cv = 30,

    [FieldOption(Label = "Cornish", Hint = "kw")]
    Kw = 31,

    [FieldOption(Label = "Corsican", Hint = "co")]
    Co = 32,

    [FieldOption(Label = "Cree", Hint = "cr")]
    Cr = 33,

    [FieldOption(Label = "Croatian", Hint = "hr")]
    Hr = 34,

    [FieldOption(Label = "Czech", Hint = "cs")]
    Cs = 35,

    [FieldOption(Label = "Danish", Hint = "da")]
    Da = 36,

    [FieldOption(Label = "Divehi", Hint = "dv")]
    Dv = 37,

    [FieldOption(Label = "Dutch", Hint = "nl")]
    Nl = 38,

    [FieldOption(Label = "Dzongkha", Hint = "dz")]
    Dz = 39,

    [FieldOption(Label = "English", Hint = "en")]
    En = 40,

    [FieldOption(Label = "Esperanto", Hint = "eo")]
    Eo = 41,

    [FieldOption(Label = "Estonian", Hint = "et")]
    Et = 42,

    [FieldOption(Label = "Ewe", Hint = "ee")]
    Ee = 43,

    [FieldOption(Label = "Faroese", Hint = "fo")]
    Fo = 44,

    [FieldOption(Label = "Fijian", Hint = "fj")]
    Fj = 45,

    [FieldOption(Label = "Finnish", Hint = "fi")]
    Fi = 46,

    [FieldOption(Label = "French", Hint = "fr")]
    Fr = 47,

    [FieldOption(Label = "Frisian", Hint = "fy")]
    Fy = 48,

    [FieldOption(Label = "Fulah", Hint = "ff")]
    Ff = 49,

    [FieldOption(Label = "Gaelic", Hint = "gd")]
    Gd = 50,

    [FieldOption(Label = "Galician", Hint = "gl")]
    Gl = 51,

    [FieldOption(Label = "Ganda", Hint = "lg")]
    Lg = 52,

    [FieldOption(Label = "Georgian", Hint = "ka")]
    Ka = 53,

    [FieldOption(Label = "German", Hint = "de")]
    De = 54,

    [FieldOption(Label = "Greek", Hint = "el")]
    El = 55,

    [FieldOption(Label = "Guarani", Hint = "gn")]
    Gn = 56,

    [FieldOption(Label = "Gujarati", Hint = "gu")]
    Gu = 57,

    [FieldOption(Label = "Haitian; Haitian Creole", Hint = "ht")]
    Ht = 58,

    [FieldOption(Label = "Hausa", Hint = "ha")]
    Ha = 59,

    [FieldOption(Label = "Hebrew", Hint = "he")]
    He = 60,

    [FieldOption(Label = "Herero", Hint = "hz")]
    Hz = 61,

    [FieldOption(Label = "Hindi", Hint = "hi")]
    Hi = 62,

    [FieldOption(Label = "Hiri Motu", Hint = "ho")]
    Ho = 63,

    [FieldOption(Label = "Hungarian", Hint = "hu")]
    Hu = 64,

    [FieldOption(Label = "Icelandic", Hint = "is")]
    Is = 65,

    [FieldOption(Label = "Ido", Hint = "io")]
    Io = 66,

    [FieldOption(Label = "Igbo", Hint = "ig")]
    Ig = 67,

    [FieldOption(Label = "Indonesian", Hint = "id")]
    Id = 68,

    [FieldOption(Label = "Interlingua", Hint = "ia")]
    Ia = 69,

    [FieldOption(Label = "Interlingue", Hint = "ie")]
    Ie = 70,

    [FieldOption(Label = "Inuktitut", Hint = "iu")]
    Iu = 71,

    [FieldOption(Label = "Inupiaq", Hint = "ik")]
    Ik = 72,

    [FieldOption(Label = "Irish", Hint = "ga")]
    Ga = 73,

    [FieldOption(Label = "Italian", Hint = "it")]
    It = 74,

    [FieldOption(Label = "Japanese", Hint = "ja")]
    Ja = 75,

    [FieldOption(Label = "Javanese", Hint = "jv")]
    Jv = 76,

    [FieldOption(Label = "Kalaallisut", Hint = "kl")]
    Kl = 77,

    [FieldOption(Label = "Kannada", Hint = "kn")]
    Kn = 78,

    [FieldOption(Label = "Kanuri", Hint = "kr")]
    Kr = 79,

    [FieldOption(Label = "Kashmiri", Hint = "ks")]
    Ks = 80,

    [FieldOption(Label = "Kazakh", Hint = "kk")]
    Kk = 81,

    [FieldOption(Label = "Khmer", Hint = "km")]
    Km = 82,

    [FieldOption(Label = "Kikuyu", Hint = "ki")]
    Ki = 83,

    [FieldOption(Label = "Kinyarwanda", Hint = "rw")]
    Rw = 84,

    [FieldOption(Label = "Kirghiz", Hint = "ky")]
    Ky = 85,

    [FieldOption(Label = "Komi", Hint = "kv")]
    Kv = 86,

    [FieldOption(Label = "Kongo", Hint = "kg")]
    Kg = 87,

    [FieldOption(Label = "Korean", Hint = "ko")]
    Ko = 88,

    [FieldOption(Label = "Kuanyama", Hint = "kj")]
    Kj = 89,

    [FieldOption(Label = "Kurdish", Hint = "ku")]
    Ku = 90,

    [FieldOption(Label = "Lao", Hint = "lo")]
    Lo = 91,

    [FieldOption(Label = "Latin", Hint = "la")]
    La = 92,

    [FieldOption(Label = "Latvian", Hint = "lv")]
    Lv = 93,

    [FieldOption(Label = "Letzeburgesch", Hint = "lb")]
    Lb = 94,

    [FieldOption(Label = "Limburgish", Hint = "li")]
    Li = 95,

    [FieldOption(Label = "Lingala", Hint = "ln")]
    Ln = 96,

    [FieldOption(Label = "Lithuanian", Hint = "lt")]
    Lt = 97,

    [FieldOption(Label = "Luba-Katanga", Hint = "lu")]
    Lu = 98,

    [FieldOption(Label = "Macedonian", Hint = "mk")]
    Mk = 99,

    [FieldOption(Label = "Malagasy", Hint = "mg")]
    Mg = 100,

    [FieldOption(Label = "Malay", Hint = "ms")]
    Ms = 101,

    [FieldOption(Label = "Malayalam", Hint = "ml")]
    Ml = 102,

    [FieldOption(Label = "Maltese", Hint = "mt")]
    Mt = 103,

    [FieldOption(Label = "Mandarin", Hint = "zh")]
    Zh = 104,

    [FieldOption(Label = "Manx", Hint = "gv")]
    Gv = 105,

    [FieldOption(Label = "Maori", Hint = "mi")]
    Mi = 106,

    [FieldOption(Label = "Marathi", Hint = "mr")]
    Mr = 107,

    [FieldOption(Label = "Marshall", Hint = "mh")]
    Mh = 108,

    [FieldOption(Label = "Moldavian", Hint = "mo")]
    Mo = 109,

    [FieldOption(Label = "Mongolian", Hint = "mn")]
    Mn = 110,

    [FieldOption(Label = "Nauru", Hint = "na")]
    Na = 111,

    [FieldOption(Label = "Navajo", Hint = "nv")]
    Nv = 112,

    [FieldOption(Label = "South Ndebele", Hint = "nr")]
    Nr = 113,

    [FieldOption(Label = "North Ndebele", Hint = "nd")]
    Nd = 114,

    [FieldOption(Label = "Ndonga", Hint = "ng")]
    Ng = 115,

    [FieldOption(Label = "Nepali", Hint = "ne")]
    Ne = 116,

    [FieldOption(Label = "No Language", Hint = "xx")]
    Xx = 117,

    [FieldOption(Label = "Northern Sami", Hint = "se")]
    Se = 118,

    [FieldOption(Label = "Norwegian", Hint = "no")]
    No = 119,

    [FieldOption(Label = "Norwegian Bokmål", Hint = "nb")]
    Nb = 120,

    [FieldOption(Label = "Norwegian Nynorsk", Hint = "nn")]
    Nn = 121,

    [FieldOption(Label = "Occitan", Hint = "oc")]
    Oc = 122,

    [FieldOption(Label = "Ojibwa", Hint = "oj")]
    Oj = 123,

    [FieldOption(Label = "Oriya", Hint = "or")]
    Or = 124,

    [FieldOption(Label = "Oromo", Hint = "om")]
    Om = 125,

    [FieldOption(Label = "Ossetian; Ossetic", Hint = "os")]
    Os = 126,

    [FieldOption(Label = "Pali", Hint = "pi")]
    Pi = 127,

    [FieldOption(Label = "Persian", Hint = "fa")]
    Fa = 128,

    [FieldOption(Label = "Polish", Hint = "pl")]
    Pl = 129,

    [FieldOption(Label = "Portuguese", Hint = "pt")]
    Pt = 130,

    [FieldOption(Label = "Punjabi", Hint = "pa")]
    Pa = 131,

    [FieldOption(Label = "Pushto", Hint = "ps")]
    Ps = 132,

    [FieldOption(Label = "Quechua", Hint = "qu")]
    Qu = 133,

    [FieldOption(Label = "Raeto-Romance", Hint = "rm")]
    Rm = 134,

    [FieldOption(Label = "Romanian", Hint = "ro")]
    Ro = 135,

    [FieldOption(Label = "Rundi", Hint = "rn")]
    Rn = 136,

    [FieldOption(Label = "Russian", Hint = "ru")]
    Ru = 137,

    [FieldOption(Label = "Samoan", Hint = "sm")]
    Sm = 138,

    [FieldOption(Label = "Sango", Hint = "sg")]
    Sg = 139,

    [FieldOption(Label = "Sanskrit", Hint = "sa")]
    Sa = 140,

    [FieldOption(Label = "Sardinian", Hint = "sc")]
    Sc = 141,

    [FieldOption(Label = "Serbian", Hint = "sr")]
    Sr = 142,

    [FieldOption(Label = "Serbo-Croatian", Hint = "sh")]
    Sh = 143,

    [FieldOption(Label = "Shona", Hint = "sn")]
    Sn = 144,

    [FieldOption(Label = "Sindhi", Hint = "sd")]
    Sd = 145,

    [FieldOption(Label = "Sinhalese", Hint = "si")]
    Si = 146,

    [FieldOption(Label = "Slavic", Hint = "cu")]
    Cu = 147,

    [FieldOption(Label = "Slovak", Hint = "sk")]
    Sk = 148,

    [FieldOption(Label = "Slovenian", Hint = "sl")]
    Sl = 149,

    [FieldOption(Label = "Somali", Hint = "so")]
    So = 150,

    [FieldOption(Label = "Sotho", Hint = "st")]
    St = 151,

    [FieldOption(Label = "Spanish", Hint = "es")]
    Es = 152,

    [FieldOption(Label = "Sundanese", Hint = "su")]
    Su = 153,

    [FieldOption(Label = "Swahili", Hint = "sw")]
    Sw = 154,

    [FieldOption(Label = "Swati", Hint = "ss")]
    Ss = 155,

    [FieldOption(Label = "Swedish", Hint = "sv")]
    Sv = 156,

    [FieldOption(Label = "Tagalog", Hint = "tl")]
    Tl = 157,

    [FieldOption(Label = "Tahitian", Hint = "ty")]
    Ty = 158,

    [FieldOption(Label = "Tajik", Hint = "tg")]
    Tg = 159,

    [FieldOption(Label = "Tamil", Hint = "ta")]
    Ta = 160,

    [FieldOption(Label = "Tatar", Hint = "tt")]
    Tt = 161,

    [FieldOption(Label = "Telugu", Hint = "te")]
    Te = 162,

    [FieldOption(Label = "Thai", Hint = "th")]
    Th = 163,

    [FieldOption(Label = "Tibetan", Hint = "bo")]
    Bo = 164,

    [FieldOption(Label = "Tigrinya", Hint = "ti")]
    Ti = 165,

    [FieldOption(Label = "Tonga", Hint = "to")]
    To = 166,

    [FieldOption(Label = "Tsonga", Hint = "ts")]
    Ts = 167,

    [FieldOption(Label = "Tswana", Hint = "tn")]
    Tn = 168,

    [FieldOption(Label = "Turkish", Hint = "tr")]
    Tr = 169,

    [FieldOption(Label = "Turkmen", Hint = "tk")]
    Tk = 170,

    [FieldOption(Label = "Twi", Hint = "tw")]
    Tw = 171,

    [FieldOption(Label = "Uighur", Hint = "ug")]
    Ug = 172,

    [FieldOption(Label = "Ukrainian", Hint = "uk")]
    Uk = 173,

    [FieldOption(Label = "Urdu", Hint = "ur")]
    Ur = 174,

    [FieldOption(Label = "Uzbek", Hint = "uz")]
    Uz = 175,

    [FieldOption(Label = "Venda", Hint = "ve")]
    Ve = 176,

    [FieldOption(Label = "Vietnamese", Hint = "vi")]
    Vi = 177,

    [FieldOption(Label = "Volapük", Hint = "vo")]
    Vo = 178,

    [FieldOption(Label = "Walloon", Hint = "wa")]
    Wa = 179,

    [FieldOption(Label = "Welsh", Hint = "cy")]
    Cy = 180,

    [FieldOption(Label = "Wolof", Hint = "wo")]
    Wo = 181,

    [FieldOption(Label = "Xhosa", Hint = "xh")]
    Xh = 182,

    [FieldOption(Label = "Yi", Hint = "ii")]
    Ii = 183,

    [FieldOption(Label = "Yiddish", Hint = "yi")]
    Yi = 184,

    [FieldOption(Label = "Yoruba", Hint = "yo")]
    Yo = 185,

    [FieldOption(Label = "Zhuang", Hint = "za")]
    Za = 186,

    [FieldOption(Label = "Zulu", Hint = "zu")]
    Zu = 187
}
