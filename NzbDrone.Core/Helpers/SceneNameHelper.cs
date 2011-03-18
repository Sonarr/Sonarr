using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Helpers
{
    public static class SceneNameHelper
    {
        private static List<SceneNameModel> _sceneNameMappings = new List<SceneNameModel>
        {
            new SceneNameModel { SeriesId = 72546, Name = "CSI" },
            new SceneNameModel { SeriesId = 73696, Name = "CSI New York" },
            new SceneNameModel { SeriesId = 73696, Name = "CSI NY" },
            new SceneNameModel { SeriesId = 110381, Name = "Archer" },
            new SceneNameModel { SeriesId = 83897, Name = "Life After People The Series" },
            new SceneNameModel { SeriesId = 83897, Name = "Life After People" },
            new SceneNameModel { SeriesId = 80552, Name = "Kitchen Nightmares US" },
            new SceneNameModel { SeriesId = 71256, Name = "The Daily Show" },
            new SceneNameModel { SeriesId = 71256, Name = "The Daily Show with Jon Stewart" },
            new SceneNameModel { SeriesId = 75692, Name = "Law and Order SVU" },
            new SceneNameModel { SeriesId = 75692, Name = "Law and Order Special Victims Unit" },
            new SceneNameModel { SeriesId = 71489, Name = "Law and Order Criminal Intent" },
            new SceneNameModel { SeriesId = 71489, Name = "Law and Order CI" },
            new SceneNameModel { SeriesId = 79590, Name = "Dancing With The Stars US" },
            new SceneNameModel { SeriesId = 73387, Name = "Craig Ferguson" },
            new SceneNameModel { SeriesId = 85355, Name = "Jimmy Fallon" },
            new SceneNameModel { SeriesId = 75088, Name = "David Letterman" },
            new SceneNameModel { SeriesId = 76706, Name = "Big Brother US" },
            new SceneNameModel { SeriesId = 105521, Name = "The Colony" },
            new SceneNameModel { SeriesId = 105521, Name = "The Colony US" },
            new SceneNameModel { SeriesId = 76235, Name = "Americas Funniest Home Videos" },
            new SceneNameModel { SeriesId = 76235, Name = "AFHV" },
            new SceneNameModel { SeriesId = 139941, Name = "Childrens Hospital US" },
            new SceneNameModel { SeriesId = 139941, Name = "Childrens Hospital" },
            new SceneNameModel { SeriesId = 83123, Name = "Merlin" },
            new SceneNameModel { SeriesId = 83123, Name = "Merlin 2008" },
            new SceneNameModel { SeriesId = 76779, Name = "WWE Monday Night RAW" },
            new SceneNameModel { SeriesId = 164951, Name = "Shit My Dad Says" },
            new SceneNameModel { SeriesId = 83714, Name = "Genius with Dave Gorman" },
            new SceneNameModel { SeriesId = 168161, Name = "Law and Order Los Angeles" },
            new SceneNameModel { SeriesId = 168161, Name = "Law and Order LA" },
            new SceneNameModel { SeriesId = 77526, Name = "Star Trek TOS" },
            new SceneNameModel { SeriesId = 72073, Name = "Star Trek DS9" },
            new SceneNameModel { SeriesId = 72194, Name = "Ellen Degeneres" },
            new SceneNameModel { SeriesId = 72194, Name = "Ellen Degeneres" },
            new SceneNameModel { SeriesId = 195831, Name = "Drinking Made Easy" },
            new SceneNameModel { SeriesId = 195831, Name = "Zane Lampreys Drinking Made Easy" },
            new SceneNameModel { SeriesId = 76133, Name = "Poirot" },
            new SceneNameModel { SeriesId = 76133, Name = "Agatha Christies Poirot" },
            new SceneNameModel { SeriesId = 70870, Name = "The Real World Road Rules Challenge" },
            new SceneNameModel { SeriesId = 70870, Name = "The Challenge Cutthroat" },
            new SceneNameModel { SeriesId = 77444, Name = "This Old House Program" },
            new SceneNameModel { SeriesId = 73290, Name = "60 Minutes US" },
            new SceneNameModel { SeriesId = 194751, Name = "Conan" },
            new SceneNameModel { SeriesId = 194751, Name = "Conan 2010" },
            new SceneNameModel { SeriesId = 164451, Name = "Carlos 2010" },
            new SceneNameModel { SeriesId = 70726, Name = "Babalon 5" },
            new SceneNameModel { SeriesId = 70726, Name = "Babalon5" },
            new SceneNameModel { SeriesId = 83714, Name = "Genius" },
            new SceneNameModel { SeriesId = 83714, Name = "Genius With Dave Gormand" },
            new SceneNameModel { SeriesId = 212571, Name = "Come Fly With Me 2010" },
            new SceneNameModel { SeriesId = 81563, Name = "Border Security" },
            new SceneNameModel { SeriesId = 81563, Name = "Border Security Australias Frontline" },
            new SceneNameModel { SeriesId = 172381, Name = "Silent Library US" },
            new SceneNameModel { SeriesId = 131791, Name = "Sci-Fi Science" },
            new SceneNameModel { SeriesId = 80646, Name = "Frontline" },
            new SceneNameModel { SeriesId = 80646, Name = "Frontline US" },
            new SceneNameModel { SeriesId = 189931, Name = "RBT AU" },
            new SceneNameModel { SeriesId = 73255, Name = "House" },
            new SceneNameModel { SeriesId = 73255, Name = "House MD" },
            new SceneNameModel { SeriesId = 73244, Name = "The Office" },
            new SceneNameModel { SeriesId = 73244, Name = "The Office US" },
        };

        public static int FindByName(string cleanSeriesName)
        {
            var map = _sceneNameMappings.Single(s => Parser.NormalizeTitle(s.Name) == cleanSeriesName);

            if (map == null)
                return 0;

            return map.SeriesId;
        }

        public static List<String> FindById(int seriesId)
        {
            List<String> results = new List<string>();

            var maps = _sceneNameMappings.Where(s => s.SeriesId == seriesId);

            foreach (var map in maps)
                results.Add(map.Name);

            return results;
        }

    }
}
