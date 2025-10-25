using System.Text.RegularExpressions;

namespace NzbDrone.Core.Parser;

// These are functions shared between different parser functions
// they are not intended to be used outside of them parsing.
internal static class ParserCommon
{
    internal static readonly RegexReplace[] PreSubstitutionRegex = new[]
    {
        // Korean series without season number, replace with S01Exxx and remove airdate
        new RegexReplace(@"\.E(\d{2,4})\.\d{6}\.(.*-(F1RST|NEXT))$", ".S01E$1.$2", RegexOptions.Compiled),

        // Some Chinese anime releases contain both English and Chinese titles, remove the Chinese title and replace with normal anime pattern
        new RegexReplace(@"^\[(?:(?<subgroup>[^\]]+?)(?:[\u4E00-\u9FCC]+)?)\]\[(?<title>[^\]]+?)(?:\s(?<chinesetitle>[\u4E00-\u9FCC][^\]]*?))\]\[(?:(?:[\u4E00-\u9FCC]+?)?(?<episode>\d{1,4})(?:[\u4E00-\u9FCC]+?)?)\]", "[${subgroup}] ${title} - ${episode} - ", RegexOptions.Compiled),

        // Chinese LoliHouse/ZERO/Lilith-Raws/Skymoon-Raws/orion origin releases don't use the expected brackets, normalize using brackets
        new RegexReplace(@"^\[(?<subgroup>[^\]]*?(?:LoliHouse|ZERO|Lilith-Raws|Skymoon-Raws|orion origin)[^\]]*?)\](?<title>[^\[\]]+?)(?: - (?<episode>[0-9-]+)\s*|\[第?(?<episode>[0-9]+(?:-[0-9]+)?)话?(?:END|完)?\])\[", "[${subgroup}][${title}][${episode}][", RegexOptions.Compiled),

        // Most Chinese anime releases contain additional brackets/separators for chinese and non-chinese titles, remove junk first and if it has S0x as season number, convert it to Sx
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s?★[^\[ -]+\s?)?\[?(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\]\[|\s*[_/·]\s*)){0,2}(?<title>[^\[\]]+?)(?:\s(?:S?(?<!\d+)((0)(?<season>\d)|(?<season>[1-9]\d))(?!\d+)))\]?(?:\[\d{4}\])?\[第?(?<episode>[0-9]+(?:-[0-9]+)?)(?:话|集)?(?: ?END|完| ?Fin)?\]", "[${subgroup}] ${title} S${season} - ${episode} ", RegexOptions.Compiled),

        // Some Chinese releases don't include a separation between Chinese and English titles within the same bracketed group
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\]\[(?<chinesetitle>(?<![^a-zA-Z0-9])[^a-zA-Z0-9]+)(?<title>[^\]]+?)\](?:\[\d{4}\])?\[第?(?<episode>[0-9]+(?:-[0-9]+)?)(?:话|集)?(?: ?END|完| ?Fin)?\]", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled),

        // Chinese season packs, that may not actually have the full season so treated as multi-episode
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s?★[^\[ -]+\s?)?\[?(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\]\[|\s*[_/·]\s*)){0,2}(?<title>[^\]]+?)\]?(?:\[\d{4}\])?\[(?:第|全)?(?<episode>[0-9]{1,4}-[0-9]{1,4})(?:话|集)?\](?:\[(?<language>.+?)\+.+?\])[_. ](?<title>.+?)S(?<season>[0-9]{1,2})(?<rest>.+?)$", "${title}S${season}E${episode}.${language}${rest}", RegexOptions.Compiled),

        // Chinese season packs, that actually have the full season so treated as multi-episode
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s?★[^\[ -]+\s?)?\[?(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\]\[|\s*[_/·]\s*)){0,2}(?<title>[^\]]+?)\]?(?:\[\d{4}\])?\[(?:全)?(?<episode>[0-9]{1,4})(?:话|集)?\](?:\[(?<language>.+?)\+.+?\])[_. ](?<title>.+?)S(?<season>[0-9]{1,2})(?<rest>.+?)$", "${title}S${season}.${language}${rest}", RegexOptions.Compiled),

        // Most Chinese anime releases contain additional brackets/separators for chinese and non-chinese titles, remove junk and replace with normal anime pattern
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s?★[^\[ -]+\s?)?\[?(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\]\[|\s*[_/·]\s*)){0,2}(?<title>[^\]]+?)\]?(?:\[\d{4}\])?\[第?(?<episode>[0-9]{1,4}(?:-[0-9]{1,4})?)(?:话|集)?(?: ?END|完| ?Fin)?\]", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled),

        // Some Chinese anime releases contain both Chinese and English titles, remove the Chinese title first and if it has S0x as season number, convert it to Sx
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s)(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\s/\s))(?<title>[^\[\]]+?)(?:\s(?:S?(?<!\d+)((0)(?<season>\d)|(?<season>[1-9]\d))(?!\d+)))(?:[- ]+)(?<episode>[0-9]+(?:-[0-9]+)?)话?(?:END|完)?", "[${subgroup}] ${title} S${season} - ${episode} ", RegexOptions.Compiled),

        // Some Chinese anime releases contain both English and Chinese titles, remove the Chinese title and replace with normal anime pattern
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s)(?:(?<title>[^\]]+?)(?:\s/\s))(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:[- ]+)(?<episode>[0-9]+(?:-[0-9]+)?(?![a-z]))话?(?:END|完)?", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled),

        // Some Chinese anime releases contain both Chinese and English titles, remove the Chinese title and replace with normal anime pattern
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s)(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\s/\s))(?<title>[^\]]+?)(?:[- ]+)(?<episode>[0-9]+(?:-[0-9]+)?(?![a-z]))话?(?:END|完)?", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled),

        // GM-Team releases with lots of square brackets
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:(?<chinesubgroup>\[(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*\])+)\[(?<title>[^\]]+?)\](?<junk>\[[^\]]+\])*\[(?<episode>[0-9]+(?:-[0-9]+)?)( END| Fin)?\]", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled),

        // Some Chinese anime releases contain both Chinese and English titles separated by | instead of /, remove the Chinese title and replace with normal anime pattern
        new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s)(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\s\|\s))(?<title>[^\]]+?)(?:[- ]+)(?<episode>[0-9]+(?:-[0-9]+)?(?![a-z]))话?(?:END|完)?", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled),

        // Spanish releases with information in brackets
        new RegexReplace(@"^(?<title>.+?(?=[ ._-]\()).+?\((?<year>\d{4})\/(?<info>S[^\/]+)", "${title} (${year}) - ${info} ", RegexOptions.Compiled),
    };

    internal static readonly RegexReplace WebsitePrefixRegex = new(@"^(?:(?:\[|\()\s*)?(?:www\.)?[-a-z0-9-]{1,256}\.(?<!Naruto-Kun\.)(?:[a-z]{2,6}\.[a-z]{2,6}|xn--[a-z0-9-]{4,}|[a-z]{2,})\b(?:\s*(?:\]|\))|[ -]{2,})[ -]*",
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    internal static readonly RegexReplace WebsitePostfixRegex = new(@"(?:\[\s*)?(?:www\.)?[-a-z0-9-]{1,256}\.(?:xn--[a-z0-9-]{4,}|[a-z]{2,6})\b(?:\s*\])$",
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    internal static readonly RegexReplace CleanTorrentSuffixRegex = new(@"\[(?:ettv|rartv|rarbg|cttv|publichd)\]$",
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
}
