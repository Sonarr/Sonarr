using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Helpers
{
    public static class EpisodeSortingHelper
    {
        private static readonly List<EpisodeSortingType> SeparatorStyles = new List<EpisodeSortingType>
                                                                               {
                                                                                   new EpisodeSortingType
                                                                                       {
                                                                                           Id = 0,
                                                                                           Name = "Dash",
                                                                                           Pattern = " - "
                                                                                       },
                                                                                   new EpisodeSortingType
                                                                                       {
                                                                                           Id = 1,
                                                                                           Name = "Space",
                                                                                           Pattern = " "
                                                                                       }
                                                                               };

        private static readonly List<EpisodeSortingType> NumberStyles = new List<EpisodeSortingType>
                                                                            {
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 0,
                                                                                        Name = "1x05",
                                                                                        Pattern = "%sx%0e"
                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 1,
                                                                                        Name = "01x05",
                                                                                        Pattern = "%0sx%0e"
                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 2,
                                                                                        Name = "S01E05",
                                                                                        Pattern = "S%0sE%0e"
                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 3,
                                                                                        Name = "s01e05",
                                                                                        Pattern = "s%0se%0e"
                                                                                    }
                                                                            };

        private static readonly List<EpisodeSortingType> MultiEpisodeStyles = new List<EpisodeSortingType>
                                                                                  {
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 0,
                                                                                              Name = "Extend",
                                                                                              Pattern = ""
                                                                                          },
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 1,
                                                                                              Name = "Duplicate",
                                                                                              Pattern = ""
                                                                                          },
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 2,
                                                                                              Name = "Repeat",
                                                                                              Pattern = ""
                                                                                          }
                                                                                  };

        public static List<EpisodeSortingType> GetSeparatorStyles()
        {
            return SeparatorStyles;
        }

        public static List<EpisodeSortingType> GetNumberStyles()
        {
            return NumberStyles;
        }

        public static List<EpisodeSortingType> GetMultiEpisodeStyles()
        {
            return MultiEpisodeStyles;
        }

        public static EpisodeSortingType GetSeparatorStyle(int id)
        {
            return SeparatorStyles.Single(s => s.Id == id);
        }

        public static EpisodeSortingType GetNumberStyle(int id)
        {
            return NumberStyles.Single(s => s.Id == id);
        }

        public static EpisodeSortingType GetMultiEpisodeStyle(int id)
        {
            return MultiEpisodeStyles.Single(s => s.Id == id);
        }

        public static EpisodeSortingType GetSeparatorStyle(string name)
        {
            return SeparatorStyles.Single(s => s.Name == name);
        }

        public static EpisodeSortingType GetNumberStyle(string name)
        {
            return NumberStyles.Single(s => s.Name == name);
        }

        public static EpisodeSortingType GetMultiEpisodeStyle(string name)
        {
            return MultiEpisodeStyles.Single(s => s.Name == name);
        }
    }
}