using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Languages;
using System.Text;

namespace NzbDrone.Core.Parser
{
    public static class Parser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Parser));

        private static readonly RegexReplace[] PreSubstitutionRegex = new[]
            {
                // Korean series without season number, replace with S01Exxx and remove airdate
                new RegexReplace(@"\.E(\d{2,4})\.\d{6}\.(.*-NEXT)$", ".S01E$1.$2", RegexOptions.Compiled),

                // Chinese LoliHouse/ZERO/Lilith-Raws releases don't use the expected brackets, normalize using brackets
                new RegexReplace(@"^\[(?<subgroup>[^\]]*?(?:LoliHouse|ZERO|Lilith-Raws)[^\]]*?)\](?<title>[^\[\]]+?)(?: - (?<episode>[0-9-]+)\s*|\[第?(?<episode>[0-9]+(?:-[0-9]+)?)话?(?:END|完)?\])\[", "[${subgroup}][${title}][${episode}][", RegexOptions.Compiled),
                
                // Most Chinese anime releases contain additional brackets/separators for chinese and non-chinese titles, remove junk and replace with normal anime pattern
                new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s?★[^\[ -]+\s?)?\[(?:(?<chinesetitle>[^\]]*?[\u4E00-\u9FCC][^\]]*?)(?:\]\[|\s*[_/·]\s*))?(?<title>[^\]]+?)\](?:\[\d{4}\])?\[第?(?<episode>[0-9]+(?:-[0-9]+)?)话?(?:END|完)?\]", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled)
            };

        private static readonly Regex[] ReportTitleRegex = new[]
            {
                //Anime - Absolute Episode Number + Title + Season+Episode
                //Todo: This currently breaks series that start with numbers
//                new Regex(@"^(?:(?<absoluteepisode>\d{2,3})(?:_|-|\s|\.)+)+(?<title>.+?)(?:\W|_)+(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)",
//                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Daily episodes without title (2018-10-12, 20181012) (Strict pattern to avoid false matches)
                new Regex(@"^(?<airyear>19[6-9]\d|20\d\d)(?<sep>[-_]?)(?<airmonth>0\d|1[0-2])\k<sep>(?<airday>[0-2]\d|3[01])(?!\d)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-Part episodes without a title (S01E05.S01E06)
                new Regex(@"^(?:\W*S?(?<season>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[ex]){1,2}(?<episode>\d{1,3}(?!\d+)))+){2,}",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes without a title, Multi (S01E04E05, 1x04x05, etc)
                new Regex(@"^(?:S?(?<season>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[-_]|[ex]){1,2}(?<episode>\d{2,3}(?!\d+))){2,})",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes without a title, Single (S01E05, 1x05)
                new Regex(@"^(?:S?(?<season>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[-_ ]?[ex])(?<episode>\d{2,3}(?!\d+))))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Episode Absolute Episode Number ([SubGroup] Series Title Episode 01)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>.+?)[-_. ](?:Episode)(?:[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:_|-|\s|\.)*?(?<hash>\[.{8}\])?(?:$|\.)?",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Absolute Episode Number + Season+Episode
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.)?)(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+(?<absoluteepisode>\d{2,3}(\.\d{1,2})?))+(?:_|-|\s|\.)+(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+).*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Season+Episode + Absolute Episode Number
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.)?)(?<title>.+?)(?:[-_\W](?<![()\[!]))+(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)(?:(?:_|-|\s|\.)+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+)))+.*?(?<hash>\[\w{8}\])?(?:$|\.)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Season+Episode
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.)?)(?<title>.+?)(?:[-_\W](?<![()\[!]))+(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)(?:\s|\.).*?(?<hash>\[\w{8}\])?(?:$|\.)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),
                
                //Anime - [SubGroup] Title with trailing number Absolute Episode Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>[^-]+?)(?:(?<![-_. ]|\b[0]\d+) - )(?:[-_. ]?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>\[\w{8}\])?(?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title with trailing number Absolute Episode Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>[^-]+?)(?:(?<![-_. ]|\b[0]\d+)[_ ]+)(?:[-_. ]?(?<absoluteepisode>\d{3}(\.\d{1,2})?(?!\d+)))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>\[\w{8}\])?(?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title - Absolute Episode Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?)(?:(?<!\b[0]\d+))(?:[. ]-[. ](?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|[-])))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>\[\w{8}\])?(?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Absolute Episode Number - Absolute Episode Number (batches without full separator between title and absolute episode numbers)
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?)(?:(?<!\b[0]\d+))(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|[-]))[. ]-[. ](?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|[-]))(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>\[\w{8}\])?(?:$|\.mkv)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Absolute Episode Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?)[-_. ]+\(?(?:[-_. ]?#?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+\)?(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>\[\w{8}\])?(?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-episode Repeated (S01E05 - S01E06, 1x05 - 1x06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<season>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[ex]|[-_. ]e){1,2}(?<episode>\d{1,3}(?!\d+)))+){2,}",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Single episodes with a title (S01E05, 1x05, etc) and trailing info in slashes
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<season>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[ex]|\W[ex]|_){1,2}(?<episode>\d{2,3}(?!\d+|(?:[ex]|\W[ex]|_|-){1,2}\d+))).+?(?:\[.+?\])(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Season EpisodeNumber + Absolute Episode Number [SubGroup]
                new Regex(@"^(?<title>.+?)(?:[-_\W](?<![()\[!]))+(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]|\W[ex]){1,2}(?<episode>(?<!\d+)\d{2}(?!\d+)))).+?(?:[-_. ]?(?<absoluteepisode>(?<!\d+)\d{3}(\.\d{1,2})?(?!\d+)))+.+?\[(?<subgroup>.+?)\](?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-Episode with a title (S01E05E06, S01E05-06, S01E05 E06, etc) and trailing info in slashes
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<season>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[ex]|\W[ex]|_){1,2}(?<episode>\d{2,3}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>\d{2,3}(?!\d+)))+).+?(?:\[.+?\])(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Absolute Episode Number [SubGroup] [Hash]? (Series Title Episode 99-100 [RlsGroup] [ABCD1234])
                new Regex(@"^(?<title>.+?)[-_. ]Episode(?:[-_. ]+(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:.+?)\[(?<subgroup>.+?)\].*?(?<hash>\[\w{8}\])?(?:$|\.)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Absolute Episode Number [SubGroup] [Hash]
                new Regex(@"^(?<title>.+?)(?:(?:_|-|\s|\.)+(?<absoluteepisode>\d{3}(\.\d{1,2})(?!\d+)))+(?:.+?)\[(?<subgroup>.+?)\].*?(?<hash>\[\w{8}\])?(?:$|\.)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Absolute Episode Number [Hash]
                new Regex(@"^(?<title>.+?)(?:(?:_|-|\s|\.)+(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:[-_. ]+(?<special>special|ova|ovd))?[-_. ]+.*?(?<hash>\[\w{8}\])(?:$|\.)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with airdate AND season/episode number, capture season/epsiode only
                new Regex(@"^(?<title>.+?)?\W*(?<airdate>\d{4}\W+[0-1][0-9]\W+[0-3][0-9])(?!\W+[0-3][0-9])[-_. ](?:s?(?<season>(?<!\d+)(?:\d{1,2})(?!\d+)))(?:[ex](?<episode>(?<!\d+)(?:\d{1,3})(?!\d+)))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with airdate AND season/episode number
                new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})\W+(?<airmonth>[0-1][0-9])\W+(?<airday>[0-3][0-9])(?!\W+[0-3][0-9]).+?(?:s?(?<season>(?<!\d+)(?:\d{1,2})(?!\d+)))(?:[ex](?<episode>(?<!\d+)(?:\d{1,3})(?!\d+)))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-episode with title (S01E05-06, S01E05-6)
                new Regex(@"^(?<title>.+?)(?:[-_\W](?<![()\[!]))+S(?<season>(?<!\d+)(?:\d{1,2})(?!\d+))E(?<episode>\d{1,2}(?!\d+))(?:-(?<episode>\d{1,2}(?!\d+)))+[-_. ]",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled), 

                //Episodes with a title, Single episodes (S01E05, 1x05, etc) & Multi-episode (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<season>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[ex]|\W[ex]){1,2}(?<episode>\d{2,3}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>\d{2,3}(?!\d+)))*)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with a title, 4 digit season number, Single episodes (S2016E05, etc) & Multi-episode (S2016E05E06, S2016E05-06, S2016E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S(?<season>(?<!\d+)(?:\d{4})(?!\d+))(?:e|\We|_){1,2}(?<episode>\d{2,3}(?!\d+))(?:(?:\-|e|\We|_){1,2}(?<episode>\d{2,3}(?!\d+)))*)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with a title, 4 digit season number, Single episodes (2016x05, etc) & Multi-episode (2016x05x06, 2016x05-06, 2016x05 x06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+(?<season>(?<!\d+)(?:\d{4})(?!\d+))(?:x|\Wx){1,2}(?<episode>\d{2,3}(?!\d+))(?:(?:\-|x|\Wx|_){1,2}(?<episode>\d{2,3}(?!\d+)))*)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-season pack
                new Regex(@"^(?<title>.+?)[-_. ]+S(?<season>(?<!\d+)(?:\d{1,2})(?!\d+))-S?(?<season>(?<!\d+)(?:\d{1,2})(?!\d+))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Partial season pack
                new Regex(@"^(?<title>.+?)(?:\W+S(?<season>(?<!\d+)(?:\d{1,2})(?!\d+))\W+(?:(?:Part\W?|(?<!\d+\W+)e)(?<seasonpart>\d{1,2}(?!\d+)))+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Mini-Series with year in title, treated as season 1, episodes are labelled as Part01, Part 01, Part.1
                new Regex(@"^(?<title>.+?\d{4})(?:\W+(?:(?:Part\W?|e)(?<episode>\d{1,2}(?!\d+)))+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Mini-Series, treated as season 1, multi episodes are labelled as E1-E2
                new Regex(@"^(?<title>.+?)(?:[-._ ][e])(?<episode>\d{2,3}(?!\d+))(?:(?:\-?[e])(?<episode>\d{2,3}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with airdate and part (2018.04.28.Part.2)
                new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airday>[0-3][0-9])(?![-_. ]+[0-3][0-9])[-_. ]+Part[-_. ]?(?<part>[1-9])",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Mini-Series, treated as season 1, episodes are labelled as Part01, Part 01, Part.1
                new Regex(@"^(?<title>.+?)(?:\W+(?:(?:Part\W?|(?<!\d+\W+)e)(?<episode>\d{1,2}(?!\d+)))+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Mini-Series, treated as season 1, episodes are labelled as Part One/Two/Three/...Nine, Part.One, Part_One
                new Regex(@"^(?<title>.+?)(?:\W+(?:Part[-._ ](?<episode>One|Two|Three|Four|Five|Six|Seven|Eight|Nine)(?>[-._ ])))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Mini-Series, treated as season 1, episodes are labelled as XofY
                new Regex(@"^(?<title>.+?)(?:\W+(?:(?<episode>(?<!\d+)\d{1,2}(?!\d+))of\d+)+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports Season 01 Episode 03
                new Regex(@"(?:.*(?:\""|^))(?<title>.*?)(?:[-_\W](?<![()\[]))+(?:\W?Season\W?)(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:\W|_)+(?:Episode\W)(?:[-_. ]?(?<episode>(?<!\d+)\d{1,2}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-episode with episodes in square brackets (Series Title [S01E11E12] or Series Title [S01E11-12])
                new Regex(@"(?:.*(?:^))(?<title>.*?)[-._ ]+\[S(?<season>(?<!\d+)\d{2}(?!\d+))(?:[E-]{1,2}(?<episode>(?<!\d+)\d{2}(?!\d+)))+\]",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-episode release with no space between series title and season (S01E11E12)
                new Regex(@"(?:.*(?:^))(?<title>.*?)S(?<season>(?<!\d+)\d{2}(?!\d+))(?:E(?<episode>(?<!\d+)\d{2}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-episode with single episode numbers (S6.E1-E2, S6.E1E2, S6E1E2, etc)
                new Regex(@"^(?<title>.+?)[-_. ]S(?<season>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:[-_. ]?[ex]?(?<episode>(?<!\d+)\d{1,2}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Single episode season or episode S1E1 or S1-E1 or S1.Ep1 or S01.Ep.01
                new Regex(@"(?:.*(?:\""|^))(?<title>.*?)(?:\W?|_)S(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:\W|_)?Ep?[ ._]?(?<episode>(?<!\d+)\d{1,2}(?!\d+))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //3 digit season S010E05
                new Regex(@"(?:.*(?:\""|^))(?<title>.*?)(?:\W?|_)S(?<season>(?<!\d+)\d{3}(?!\d+))(?:\W|_)?E(?<episode>(?<!\d+)\d{1,2}(?!\d+))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //5 digit episode number with a title
                new Regex(@"^(?:(?<title>.+?)(?:_|-|\s|\.)+)(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+)))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>(?<!\d+)\d{5}(?!\d+)))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //5 digit multi-episode with a title
                new Regex(@"^(?:(?<title>.+?)(?:_|-|\s|\.)+)(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+)))(?:(?:[-_. ]{1,3}ep){1,2}(?<episode>(?<!\d+)\d{5}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Separated season and episode numbers S01 - E01
                new Regex(@"^(?<title>.+?)(?:_|-|\s|\.)+S(?<season>\d{2}(?!\d+))(\W-\W)E(?<episode>(?<!\d+)\d{2}(?!\d+))(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Season and episode numbers in square brackets (single and mult-episode)
                // Series Title - [02x01] - Episode 1
                // Series Title - [02x01x02] - Episode 1
                new Regex(@"^(?<title>.+?)?(?:[-_\W](?<![()\[!]))+\[(?<season>(?<!\d+)\d{1,2})(?:(?:-|x){1,2}(?<episode>\d{2}))+\].+?(?:\.|$)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled), 

                // Anime - Title with season number - Absolute Episode Number (Title S01 - EP14)
                new Regex(@"^(?<title>.+?S\d{1,2})[-_. ]{3,}(?:EP)?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|[-]))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - French titles with single episode numbers, with or without leading sub group ([RlsGroup] Title - Episode 1)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)[-_. ]+?(?:Episode[-_. ]+?)(?<absoluteepisode>\d{1}(\.\d{1,2})?(?!\d+))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Season only releases
                new Regex(@"^(?<title>.+?)\W(?:S|Season|Saison)\W?(?<season>\d{1,2}(?!\d+))(\W+|_|$)(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //4 digit season only releases
                new Regex(@"^(?<title>.+?)\W(?:S|Season|Saison)\W?(?<season>\d{4}(?!\d+))(\W+|_|$)(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with a title and season/episode in square brackets
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+\[S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>(?<!\d+)\d{2}(?!\d+|i|p)))+\])\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports 103/113 naming
                new Regex(@"^(?<title>.+?)?(?:(?:[_.-](?<![()\[!]))+(?<season>(?<!\d+)[1-9])(?<episode>[1-9][0-9]|[0][1-9])(?![a-z]|\d+))+(?:[_.]|$)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //4 digit episode number
                //Episodes without a title, Single (S01E05, 1x05) AND Multi (S01E04E05, 1x04x05, etc)
                new Regex(@"^(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>\d{4}(?!\d+|i|p)))+)(\W+|_|$)(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //4 digit episode number
                //Episodes with a title, Single episodes (S01E05, 1x05, etc) & Multi-episode (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>\d{4}(?!\d+|i|p)))+)\W?(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with airdate (2018.04.28)
                new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airday>[0-3][0-9])(?![-_. ]+[0-3][0-9])",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with airdate (04.28.2018)
                new Regex(@"^(?<title>.+?)?\W*(?<airmonth>[0-1][0-9])[-_. ]+(?<airday>[0-3][0-9])[-_. ]+(?<airyear>\d{4})(?!\d+)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with airdate (20180428)
                new Regex(@"^(?<title>.+?)?\W*(?<!\d+)(?<airyear>\d{4})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9])(?!\d+)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports 1103/1113 naming
                new Regex(@"^(?<title>.+?)?(?:(?:[-_. ](?<![()\[!]))*(?<season>(?<!\d+|\(|\[|e|x)\d{2})(?<episode>(?<!e|x)\d{2}(?!p|i|\d+|\)|\]|\W\d+|\W(?:e|ep|x)\d+)))+([-_. ]+|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with single digit episode number (S01E1, S01E5E6, etc)
                new Regex(@"^(?<title>.*?)(?:(?:[-_\W](?<![()\[!]))+S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]){1,2}(?<episode>\d{1}))+)+(\W+|_|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //iTunes Season 1\05 Title (Quality).ext
                new Regex(@"^(?:Season(?:_|-|\s|\.)(?<season>(?<!\d+)\d{1,2}(?!\d+)))(?:_|-|\s|\.)(?<episode>(?<!\d+)\d{1,2}(?!\d+))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //iTunes 1-05 Title (Quality).ext
                new Regex(@"^(?:(?<season>(?<!\d+)(?:\d{1,2})(?!\d+))(?:-(?<episode>\d{2,3}(?!\d+))))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime Range - Title Absolute Episode Number (ep01-12)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:_|\s|\.)+(?:e|ep)(?<absoluteepisode>\d{2,3}(\.\d{1,2})?)-(?<absoluteepisode>(?<!\d+)\d{1,2}(\.\d{1,2})?(?!\d+|-)).*?(?<hash>\[\w{8}\])?(?:$|\.)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Absolute Episode Number (e66)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:(?:_|-|\s|\.)+(?:e|ep)(?<absoluteepisode>\d{2,4}(\.\d{1,2})?))+.*?(?<hash>\[\w{8}\])?(?:$|\.)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Episode Absolute Episode Number (Series Title Episode 01)
                new Regex(@"^(?<title>.+?)[-_. ](?:Episode)(?:[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:_|-|\s|\.)*?(?<hash>\[.{8}\])?(?:$|\.)?",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime Range - Title Absolute Episode Number (1 or 2 digit absolute episode numbers in a range, 1-10)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)[_. ]+(?<absoluteepisode>(?<!\d+)\d{1,2}(\.\d{1,2})?(?!\d+))-(?<absoluteepisode>(?<!\d+)\d{1,2}(\.\d{1,2})?(?!\d+|-))(?:_|\s|\.)*?(?<hash>\[.{8}\])?(?:$|\.)?",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Absolute Episode Number
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+|[ip])))+(?:_|-|\s|\.)*?(?<hash>\[.{8}\])?(?:$|\.)?",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title {Absolute Episode Number}
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+|[ip])))+(?:_|-|\s|\.)*?(?<hash>\[.{8}\])?(?:$|\.)?",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Extant, terrible multi-episode naming (extant.10708.hdtv-lol.mp4)
                new Regex(@"^(?<title>.+?)[-_. ](?<season>[0]?\d?)(?:(?<episode>\d{2}){2}(?!\d+))[-_. ]",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };


        private static readonly Regex[] SpecialEpisodeTitleRegex = new Regex[]
            {
                new Regex(@"\.S\d+E00\.(?<episodetitle>.+?)(?:\.(?:720p|1080p|HDTV|WEB|WEBRip|WEB-DL)\.|$)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                new Regex(@"\.S\d+\.Special\.(?<episodetitle>.+?)(?:\.(?:720p|1080p|HDTV|WEB|WEBRip|WEB-DL)\.|$)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

        private static readonly Regex[] RejectHashedReleasesRegexes = new Regex[]
            {
                // Generic match for md5 and mixed-case hashes.
                new Regex(@"^[0-9a-zA-Z]{32}", RegexOptions.Compiled),

                // Generic match for shorter lower-case hashes.
                new Regex(@"^[a-z0-9]{24}$", RegexOptions.Compiled),

                // Format seen on some NZBGeek releases
                // Be very strict with these coz they are very close to the valid 101 ep numbering.
                new Regex(@"^[A-Z]{11}\d{3}$", RegexOptions.Compiled),
                new Regex(@"^[a-z]{12}\d{3}$", RegexOptions.Compiled),

                //Backup filename (Unknown origins)
                new Regex(@"^Backup_\d{5,}S\d{2}-\d{2}$", RegexOptions.Compiled),

                //123 - Started appearing December 2014
                new Regex(@"^123$", RegexOptions.Compiled),

                //abc - Started appearing January 2015
                new Regex(@"^abc$", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                //abc - Started appearing 2020
                new Regex(@"^abc[-_. ]xyz", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                //b00bs - Started appearing January 2015
                new Regex(@"^b00bs$", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // 170424_26 - Started appearing August 2018
                new Regex(@"^\d{6}_\d{2}$"),

                // additional Generic match for mixed-case hashes. - Started appearing Dec 2020
                new Regex(@"^[0-9a-zA-Z]{30}", RegexOptions.Compiled),

                // additional Generic match for mixed-case hashes. - Started appearing Jan 2021
                new Regex(@"^[0-9a-zA-Z]{26}", RegexOptions.Compiled),

                // additional Generic match for mixed-case hashes. - Started appearing Jan 2021
                new Regex(@"^[0-9a-zA-Z]{39}", RegexOptions.Compiled),
            };

        private static readonly Regex[] SeasonFolderRegexes = new Regex[]
            {
                new Regex(@"^(Season[ ._-]*\d+|Specials)$", RegexOptions.Compiled)
            };

        //Regex to detect whether the title was reversed.
        private static readonly Regex ReversedTitleRegex = new Regex(@"(?:^|[-._ ])(p027|p0801|\d{2,3}E\d{2}S)[-._ ]", RegexOptions.Compiled);

        private static readonly RegexReplace NormalizeRegex = new RegexReplace(@"((?:\b|_)(?<!^)(a(?!$)|an|the|and|or|of)(?:\b|_))|\W|_",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PercentRegex = new Regex(@"(?<=\b\d+)%", RegexOptions.Compiled);

        private static readonly Regex FileExtensionRegex = new Regex(@"\.[a-z0-9]{2,4}$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace SimpleTitleRegex = new RegexReplace(@"(?:(480|720|1080|2160)[ip]|[xh][\W_]?26[45]|DD\W?5\W1|[<>?*:|]|848x480|1280x720|1920x1080|3840x2160|4096x2160|(8|10)b(it)?|10-bit)\s*?",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace WebsitePrefixRegex = new RegexReplace(@"^\[\s*[-a-z]+(\.[a-z]+)+\s*\][- ]*|^www\.[a-z]+\.(?:com|net|org)[ -]*",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace WebsitePostfixRegex = new RegexReplace(@"\[\s*[-a-z]+(\.[a-z0-9]+)+\s*\]$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SixDigitAirDateRegex = new Regex(@"(?<=[_.-])(?<airdate>(?<!\d)(?<airyear>[1-9]\d{1})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9]))(?=[_.-])",
                                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace CleanReleaseGroupRegex = new RegexReplace(@"^(.*?[-._ ](S\d+E\d+)[-._ ])|(-(RP|1|NZBGeek|Obfuscated|Scrambled|sample|Pre|postbot|xpost|Rakuv[a-z0-9]*|WhiteRev|BUYMORE|AsRequested|AlternativeToRequested|GEROV|Z0iDS3N|Chamele0n|4P|4Planet|AlteZachen))+$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace CleanTorrentSuffixRegex = new RegexReplace(@"\[(?:ettv|rartv|rarbg|cttv)\]$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CleanQualityBracketsRegex = new Regex(@"\[[a-z0-9 ._-]+\]$",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReleaseGroupRegex = new Regex(@"-(?<releasegroup>[a-z0-9]+(?!.+?(?:480p|720p|1080p|2160p)))(?<!.*?WEB-DL|Blu-Ray|480p|720p|1080p|2160p|DTS-HD|DTS-X|DTS-MA|DTS-ES)(?:\b|[-._ ]|$)|[-._ ]\[(?<releasegroup>[a-z0-9]+)\]$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);


        private static readonly Regex InvalidReleaseGroupRegex = new Regex(@"^[se]\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        private static readonly Regex AnimeReleaseGroupRegex = new Regex(@"^(?:\[(?<subgroup>(?!\s).+?(?<!\s))\](?:_|-|\s|\.)?)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_)(?<italian>\b(?:ita|italian)\b)|(?<german>german\b|videomann)|(?<flemish>flemish)|(?<greek>greek)|(?<french>(?:\W|_)(?:FR|VOSTFR)(?:\W|_))|(?<russian>\brus\b)|(?<dutch>nl\W?subs?)|(?<hungarian>\b(?:HUNDUB|HUN)\b)|(?<spanish>\b(?:español|castellano)\b)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex YearInTitleRegex = new Regex(@"^(?<title>.+?)(?:\W|_)?(?<year>\d{4})",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TitleComponentsRegex = new Regex(@"^(?<title>.+?) \((?<title>.+?)\)$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex WordDelimiterRegex = new Regex(@"(\s|\.|,|_|-|=|\|)+", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex CommonWordRegex = new Regex(@"\b(a|an|the|and|or|of)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SpecialEpisodeWordRegex = new Regex(@"\b(part|special|edition|christmas)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        private static readonly Regex RequestInfoRegex = new Regex(@"^(?:\[.+?\])+", RegexOptions.Compiled);

        private static readonly string[] Numbers = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

        public static ParsedEpisodeInfo ParsePath(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseTitle(fileInfo.Name);

            if (result == null)
            {
                Logger.Debug("Attempting to parse episode info using directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseTitle(fileInfo.Directory.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                Logger.Debug("Attempting to parse episode info using directory name. {0}", fileInfo.Directory.Name);
                result = ParseTitle(fileInfo.Directory.Name + fileInfo.Extension);
            }

            return result;
        }

        public static ParsedEpisodeInfo ParseTitle(string title)
        {
            try
            {
                if (!ValidateBeforeParsing(title)) return null;

                Logger.Debug("Parsing string '{0}'", title);

                if (ReversedTitleRegex.IsMatch(title))
                {
                    var titleWithoutExtension = RemoveFileExtension(title).ToCharArray();
                    Array.Reverse(titleWithoutExtension);

                    title = new string(titleWithoutExtension) + title.Substring(titleWithoutExtension.Length);

                    Logger.Debug("Reversed name detected. Converted to '{0}'", title);
                }

                var releaseTitle = RemoveFileExtension(title);

                releaseTitle = releaseTitle.Replace("【", "[").Replace("】", "]");

                foreach (var replace in PreSubstitutionRegex)
                {
                    if (replace.TryReplace(ref releaseTitle))
                    {
                        Logger.Debug("Substituted with " + releaseTitle);
                    }
                }

                var simpleTitle = SimpleTitleRegex.Replace(releaseTitle);

                // TODO: Quick fix stripping [url] - prefixes and postfixes.
                simpleTitle = WebsitePrefixRegex.Replace(simpleTitle);
                simpleTitle = WebsitePostfixRegex.Replace(simpleTitle);

                simpleTitle = CleanTorrentSuffixRegex.Replace(simpleTitle);

                simpleTitle = CleanQualityBracketsRegex.Replace(simpleTitle, m =>
                {
                    if (QualityParser.ParseQualityName(m.Value).Quality != Qualities.Quality.Unknown)
                    {
                        return string.Empty;
                    }

                    return m.Value;
                });

                var sixDigitAirDateMatch = SixDigitAirDateRegex.Match(simpleTitle);
                if (sixDigitAirDateMatch.Success)
                {
                    var airYear = sixDigitAirDateMatch.Groups["airyear"].Value;
                    var airMonth = sixDigitAirDateMatch.Groups["airmonth"].Value;
                    var airDay = sixDigitAirDateMatch.Groups["airday"].Value;

                    if (airMonth != "00" || airDay != "00")
                    {
                        var fixedDate = string.Format("20{0}.{1}.{2}", airYear, airMonth, airDay);

                        simpleTitle = simpleTitle.Replace(sixDigitAirDateMatch.Groups["airdate"].Value, fixedDate);
                    }
                }

                foreach (var regex in ReportTitleRegex)
                {
                    var match = regex.Matches(simpleTitle);

                    if (match.Count != 0)
                    {
                        Logger.Trace(regex);
                        try
                        {
                            var result = ParseMatchCollection(match, releaseTitle);

                            if (result != null)
                            {
                                if (result.FullSeason && result.ReleaseTokens.ContainsIgnoreCase("Special"))
                                {
                                    result.FullSeason = false;
                                    result.Special = true;
                                }

                                result.Language = LanguageParser.ParseLanguage(releaseTitle);
                                Logger.Debug("Language parsed: {0}", result.Language);

                                result.Quality = QualityParser.ParseQuality(title);
                                Logger.Debug("Quality parsed: {0}", result.Quality);

                                result.ReleaseGroup = ParseReleaseGroup(releaseTitle);

                                var subGroup = GetSubGroup(match);
                                if (!subGroup.IsNullOrWhiteSpace())
                                {
                                    result.ReleaseGroup = subGroup;
                                }

                                Logger.Debug("Release Group parsed: {0}", result.ReleaseGroup);

                                result.ReleaseHash = GetReleaseHash(match);
                                if (!result.ReleaseHash.IsNullOrWhiteSpace())
                                {
                                    Logger.Debug("Release Hash parsed: {0}", result.ReleaseHash);
                                }

                                return result;
                            }
                        }
                        catch (InvalidDateException ex)
                        {
                            Logger.Debug(ex, ex.Message);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!title.ToLower().Contains("password") && !title.ToLower().Contains("yenc"))
                    Logger.Error(e, "An error has occurred while trying to parse {0}", title);
            }

            Logger.Debug("Unable to parse {0}", title);
            return null;
        }

        public static string ParseSeriesName(string title)
        {
            Logger.Debug("Parsing string '{0}'", title);

            var parseResult = ParseTitle(title);

            if (parseResult == null)
            {
                return CleanSeriesTitle(title);
            }

            return parseResult.SeriesTitle;
        }

        public static string CleanSeriesTitle(this string title)
        {
            long number = 0;

            //If Title only contains numbers return it as is.
            if (long.TryParse(title, out number))
                return title;

            // Replace `%` with `percent` to deal with the 3% case
            title = PercentRegex.Replace(title, "percent");

            return NormalizeRegex.Replace(title).ToLower().RemoveAccent();
        }

        public static string NormalizeEpisodeTitle(string title)
        {
            var match = SpecialEpisodeTitleRegex
                        .Select(v => v.Match(title))
                        .FirstOrDefault(v => v.Success);

            if (match != null)
            {
                title = match.Groups["episodetitle"].Value;
            }

            // Disabled, Until we run into specific testcases for the removal of these words.
            //title = SpecialEpisodeWordRegex.Replace(title, string.Empty);

            title = PunctuationRegex.Replace(title, " ");
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim()
                        .ToLower();
        }

        public static string NormalizeTitle(string title)
        {
            title = WordDelimiterRegex.Replace(title, " ");
            title = PunctuationRegex.Replace(title, string.Empty);
            title = CommonWordRegex.Replace(title, string.Empty);
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim().ToLower();
        }

        public static string ParseReleaseGroup(string title)
        {
            title = title.Trim();
            title = RemoveFileExtension(title);
            foreach (var replace in PreSubstitutionRegex)
            {
                if (replace.TryReplace(ref title))
                    break;
            }
            title = WebsitePrefixRegex.Replace(title);

            var animeMatch = AnimeReleaseGroupRegex.Match(title);

            if (animeMatch.Success)
            {
                return animeMatch.Groups["subgroup"].Value;
            }

            title = CleanReleaseGroupRegex.Replace(title);

            var matches = ReleaseGroupRegex.Matches(title);

            if (matches.Count != 0)
            {
                var group = matches.OfType<Match>().Last().Groups["releasegroup"].Value;
                int groupIsNumeric;

                if (int.TryParse(group, out groupIsNumeric))
                {
                    return null;
                }

                if (InvalidReleaseGroupRegex.IsMatch(group))
                {
                    return null;
                }

                return group;
            }

            return null;
        }

        public static string RemoveFileExtension(string title)
        {
            title = FileExtensionRegex.Replace(title, m =>
                {
                    var extension = m.Value.ToLower();
                    if (MediaFiles.MediaFileExtensions.Extensions.Contains(extension) || new[] { ".par2", ".nzb" }.Contains(extension))
                    {
                        return string.Empty;
                    }
                    return m.Value;
                });

            return title;
        }

        private static SeriesTitleInfo GetSeriesTitleInfo(string title)
        {
            var seriesTitleInfo = new SeriesTitleInfo();
            seriesTitleInfo.Title = title;

            var match = YearInTitleRegex.Match(title);

            if (!match.Success)
            {
                seriesTitleInfo.TitleWithoutYear = title;
            }
            else
            {
                seriesTitleInfo.TitleWithoutYear = match.Groups["title"].Value;
                seriesTitleInfo.Year = Convert.ToInt32(match.Groups["year"].Value);
            }

            var matchComponents = TitleComponentsRegex.Match(seriesTitleInfo.TitleWithoutYear);

            if (matchComponents.Success)
            {
                seriesTitleInfo.AllTitles = matchComponents.Groups["title"].Captures.OfType<Capture>().Select(v => v.Value).ToArray();
            }

            return seriesTitleInfo;
        }

        private static ParsedEpisodeInfo ParseMatchCollection(MatchCollection matchCollection, string releaseTitle)
        {
            var seriesName = matchCollection[0].Groups["title"].Value.Replace('.', ' ').Replace('_', ' ');
            seriesName = RequestInfoRegex.Replace(seriesName, "").Trim(' ');

            int airYear;
            int.TryParse(matchCollection[0].Groups["airyear"].Value, out airYear);

            int lastSeasonEpisodeStringIndex = matchCollection[0].Groups["title"].EndIndex();

            ParsedEpisodeInfo result;

            if (airYear < 1900)
            {
                var seasons = new List<int>();

                foreach (Capture seasonCapture in matchCollection[0].Groups["season"].Captures)
                {
                    int parsedSeason;
                    if (int.TryParse(seasonCapture.Value, out parsedSeason))
                    {
                        seasons.Add(parsedSeason);

                        lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, seasonCapture.EndIndex());
                    }
                }

                //If no season was found it should be treated as a mini series and season 1
                if (seasons.Count == 0) seasons.Add(1);

                result = new ParsedEpisodeInfo
                {
                    ReleaseTitle = releaseTitle,
                    SeasonNumber = seasons.First(),
                    EpisodeNumbers = new int[0],
                    AbsoluteEpisodeNumbers = new int[0]
                };


                //If more than 1 season was parsed set IsMultiSeason to true so it can be rejected later
                if (seasons.Distinct().Count() > 1)
                {
                    result.IsMultiSeason = true;
                }

                foreach (Match matchGroup in matchCollection)
                {
                    var episodeCaptures = matchGroup.Groups["episode"].Captures.Cast<Capture>().ToList();
                    var absoluteEpisodeCaptures = matchGroup.Groups["absoluteepisode"].Captures.Cast<Capture>().ToList();

                    //Allows use to return a list of 0 episodes (We can handle that as a full season release)
                    if (episodeCaptures.Any())
                    {
                        var first = ParseNumber(episodeCaptures.First().Value);
                        var last = ParseNumber(episodeCaptures.Last().Value);

                        if (first > last)
                        {
                            return null;
                        }

                        var count = last - first + 1;
                        result.EpisodeNumbers = Enumerable.Range(first, count).ToArray();

                        lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, episodeCaptures.Last().EndIndex());
                    }

                    if (absoluteEpisodeCaptures.Any())
                    {
                        var first = ParseDecimal(absoluteEpisodeCaptures.First().Value);
                        var last = ParseDecimal(absoluteEpisodeCaptures.Last().Value);

                        if (first > last)
                        {
                            return null;
                        }

                        if ((first % 1) != 0 || (last % 1) != 0)
                        {
                            if (absoluteEpisodeCaptures.Count != 1)
                                return null; // Multiple matches not allowed for specials

                            result.SpecialAbsoluteEpisodeNumbers = new decimal[] { first };
                            result.Special = true;

                            lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, absoluteEpisodeCaptures.First().EndIndex());
                        }
                        else
                        {
                            var count = last - first + 1;
                            result.AbsoluteEpisodeNumbers = Enumerable.Range((int)first, (int)count).ToArray();

                            if (matchGroup.Groups["special"].Success)
                            {
                                result.Special = true;
                            }

                            lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, absoluteEpisodeCaptures.Last().EndIndex());
                        }
                    }

                    if (!episodeCaptures.Any() && !absoluteEpisodeCaptures.Any())
                    {
                        //Check to see if this is an "Extras" or "SUBPACK" release, if it is, set
                        // IsSeasonExtra so they can be filtered out
                        if (!matchCollection[0].Groups["extras"].Value.IsNullOrWhiteSpace())
                        {
                            result.IsSeasonExtra = true;
                        }

                        // Partial season packs will have a seasonpart group so they can be differentiated
                        // from a full season/single episode release
                        var seasonPart = matchCollection[0].Groups["seasonpart"].Value;

                        if (seasonPart.IsNotNullOrWhiteSpace())
                        {
                            result.SeasonPart = Convert.ToInt32(seasonPart);
                            result.IsPartialSeason = true;
                        }
                        else
                        {
                            result.FullSeason = true;
                        }
                    }
                }

                if (result.AbsoluteEpisodeNumbers.Any() && !result.EpisodeNumbers.Any())
                {
                    result.SeasonNumber = 0;
                }
            }

            else
            {
                //Try to Parse as a daily show
                var airmonth = Convert.ToInt32(matchCollection[0].Groups["airmonth"].Value);
                var airday = Convert.ToInt32(matchCollection[0].Groups["airday"].Value);

                //Swap day and month if month is bigger than 12 (scene fail)
                if (airmonth > 12)
                {
                    var tempDay = airday;
                    airday = airmonth;
                    airmonth = tempDay;
                }

                DateTime airDate;

                try
                {
                    airDate = new DateTime(airYear, airmonth, airday);
                }
                catch (Exception)
                {
                    throw new InvalidDateException("Invalid date found: {0}-{1}-{2}", airYear, airmonth, airday);
                }

                //Check if episode is in the future (most likely a parse error)
                if (airDate > DateTime.Now.AddDays(1).Date || airDate < new DateTime(1970, 1, 1))
                {
                    throw new InvalidDateException("Invalid date found: {0}", airDate);
                }

                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airyear"].EndIndex());
                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airmonth"].EndIndex());
                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airday"].EndIndex());

                result = new ParsedEpisodeInfo
                {
                    ReleaseTitle = releaseTitle,
                    AirDate = airDate.ToString(Episode.AIR_DATE_FORMAT),
                };

                var partMatch = matchCollection[0].Groups["part"];

                if (partMatch.Success)
                {
                    result.DailyPart = Convert.ToInt32(partMatch.Value);
                }
            }

            if (lastSeasonEpisodeStringIndex != releaseTitle.Length)
            {
                result.ReleaseTokens = releaseTitle.Substring(lastSeasonEpisodeStringIndex);
            }
            else
            {
                result.ReleaseTokens = releaseTitle;
            }

            result.SeriesTitle = seriesName;
            result.SeriesTitleInfo = GetSeriesTitleInfo(result.SeriesTitle);

            Logger.Debug("Episode Parsed. {0}", result);

            return result;
        }

        private static bool ValidateBeforeParsing(string title)
        {
            if (title.ToLower().Contains("password") && title.ToLower().Contains("yenc"))
            {
                Logger.Debug("");
                return false;
            }

            if (!title.Any(char.IsLetterOrDigit))
            {
                return false;
            }

            var titleWithoutExtension = RemoveFileExtension(title);

            if (RejectHashedReleasesRegexes.Any(v => v.IsMatch(titleWithoutExtension)))
            {
                Logger.Debug("Rejected Hashed Release Title: " + title);
                return false;
            }

            if (SeasonFolderRegexes.Any(v => v.IsMatch(titleWithoutExtension)))
            {
                Logger.Debug("Rejected Season Folder Release Title: " + title);
                return false;
            }

            return true;
        }

        private static string GetSubGroup(MatchCollection matchCollection)
        {
            var subGroup = matchCollection[0].Groups["subgroup"];

            if (subGroup.Success)
            {
                return subGroup.Value;
            }

            return string.Empty;
        }

        private static string GetReleaseHash(MatchCollection matchCollection)
        {
            var hash = matchCollection[0].Groups["hash"];

            if (hash.Success)
            {
                var hashValue = hash.Value.Trim('[', ']');

                if (hashValue.Equals("1280x720"))
                {
                    return string.Empty;
                }

                return hashValue;
            }

            return string.Empty;
        }

        private static int ParseNumber(string value)
        {
            int number;

            var normalized = value.Normalize(NormalizationForm.FormKC);

            if (int.TryParse(normalized, out number))
            {
                return number;
            }

            number = Array.IndexOf(Numbers, value.ToLower());

            if (number != -1)
            {
                return number;
            }

            throw new FormatException(string.Format("{0} isn't a number", value));
        }

        private static decimal ParseDecimal(string value)
        {
            decimal number;

            var normalized = value.Normalize(NormalizationForm.FormKC);

            if (decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            {
                return number;
            }


            throw new FormatException(string.Format("{0} isn't a number", value));
        }
    }
}
