using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Helpers
{
    public static class SceneNameHelper
    {
        //Todo: Move this to a publically available location (so updates can be applied without releasing a new version of NzbDrone)
        //Todo: GoogleDocs? WCF Web Services on NzbDrone.com?
        private static readonly Dictionary<String, Int32> SeriesIdLookupList = new Dictionary<string, int>();
        private static readonly Dictionary<Int32, String> SceneNameLookupList = new Dictionary<Int32, String>();


        static SceneNameHelper()
        {
            //These values are used to match report titles parsed out of RSS to a series in the DB
            SeriesIdLookupList.Add(Parser.NormalizeTitle("CSI"), 72546);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("CSI New York"), 73696);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("CSI NY"), 73696);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Archer"), 110381);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Life After People The Series"), 83897);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Life After People"), 83897);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Kitchen Nightmares US"), 80552);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("The Daily Show"), 71256);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("The Daily Show with Jon Stewart"), 71256);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Law and Order SVU"), 75692);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Law and Order Special Victims Unit"), 75692);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Law and Order Criminal Intent"), 71489);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Law and Order CI"), 71489);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Dancing With The Stars US"), 79590);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Craig Ferguson"), 73387);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Jimmy Fallon"), 85355);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("David Letterman"), 75088);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Big Brother US"), 76706);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("The Colony"), 105521);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("The Colony US"), 105521);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Americas Funniest Home Videos"), 76235);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("AFHV"), 76235);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Childrens Hospital US"), 139941);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Childrens Hospital"), 139941);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Merlin"), 83123);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Merlin 2008"), 83123);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("WWE Monday Night RAW"), 76779);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Shit My Dad Says"), 164951);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Genius with Dave Gorman"), 83714);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Law and Order LA"), 168161);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Star Trek TOS"), 77526);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Star Trek DS9"), 72073);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Ellen Degeneres"), 72194);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Drinking Made Easy"), 195831);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Zane Lampreys Drinking Made Easy"), 195831);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Poirot"), 76133);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Agatha Christies Poirot"), 76133);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("The Real World Road Rules Challenge"), 70870);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("The Challenge Cutthroat"), 70870);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("This Old House Program"), 77444);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("60 Minutes US"), 73290);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Conan"), 194751);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Conan 2010"), 194751);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Carlos 2010"), 164451);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Babalon 5"), 70726);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Babalon5"), 70726);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Genius"), 83714);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Genius With Dave Gormand"), 83714);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Come Fly With Me 2010"), 212571);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Border Security"), 81563);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Border Security Australias Frontline"), 81563);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Silent Library US"), 172381);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Sci-Fi Science"), 131791);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Frontline"), 80646);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("Frontline US"), 80646);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("RBT AU"), 189931);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("House"), 73255);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("House MD"), 73255);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("The Office"), 73244);
            SeriesIdLookupList.Add(Parser.NormalizeTitle("The Office US"), 73244);

            //These values are used when doing an indexer search.
            SceneNameLookupList.Add(72546, "CSI"); //CSI
            SceneNameLookupList.Add(73696, "CSI"); //CSI NY
            SceneNameLookupList.Add(110381, "Archer");
            SceneNameLookupList.Add(83897, "Life After People");
            SceneNameLookupList.Add(80552, "Kitchen Nightmares US");
            SceneNameLookupList.Add(71256, "The Daily Show"); //The Daily Show with Jon Stewart
            SceneNameLookupList.Add(75692, "Law and Order"); //SVU
            SceneNameLookupList.Add(71489, "Law and Order");//CI
            SceneNameLookupList.Add(79590, "Dancing With The Stars US");
            SceneNameLookupList.Add(73387, "Craig Ferguson");
            SceneNameLookupList.Add(85355, "Jimmy Fallon");
            SceneNameLookupList.Add(75088, "David Letterman");
            SceneNameLookupList.Add(76706, "Big Brother US");
            SceneNameLookupList.Add(105521, "The Colony");
            SceneNameLookupList.Add(76235, "Americas Funniest Home Videos");
            SceneNameLookupList.Add(139941, "Childrens Hospital");
            SceneNameLookupList.Add(83123, "Merlin");
            SceneNameLookupList.Add(76779, "WWE Monday Night RAW");
            SceneNameLookupList.Add(164951, "Shit My Dad Says");
            SceneNameLookupList.Add(168161, "Law and Order LA");
            SceneNameLookupList.Add(77526, "Star Trek TOS");
            SceneNameLookupList.Add(72073, "Star Trek DS9");
            SceneNameLookupList.Add(72194, "Ellen Degeneres");
            SceneNameLookupList.Add(195831, "Drinking Made Easy");//Zane Lampreys Drinking Made Easy
            SceneNameLookupList.Add(76133, "Poirot"); //Agatha Christies Poirot
            SceneNameLookupList.Add(70870, "The Real World Road Rules Challenge");
            SceneNameLookupList.Add(77444, "This Old House Program");
            SceneNameLookupList.Add(73290, "60 Minutes US");
            SceneNameLookupList.Add(194751, "Conan");
            SceneNameLookupList.Add(164451, "Carlos 2010");
            SceneNameLookupList.Add(70726, "Babalon"); //5
            SceneNameLookupList.Add(83714, "Genius"); //Genius With Dave Gormand
            SceneNameLookupList.Add(212571, "Come Fly With Me 2010");
            SceneNameLookupList.Add(81563, "Border Security");
            SceneNameLookupList.Add(172381, "Silent Library US");
            SceneNameLookupList.Add(131791, "Sci-Fi Science");
            SceneNameLookupList.Add(80646, "Frontline");
            SceneNameLookupList.Add(189931, "RBT AU");
            SceneNameLookupList.Add(73255, "House");
            SceneNameLookupList.Add(73244, "The Office");
        }


        public static Nullable<Int32> GetIdByName(string cleanSeriesName)
        {
            int id;

            if (SeriesIdLookupList.TryGetValue(Parser.NormalizeTitle(cleanSeriesName), out id))
            {
                return id;
            }

            return null;
        }

        public static String GetTitleById(int seriesId)
        {
            string title;

            if (SceneNameLookupList.TryGetValue(seriesId, out title))
            {
                return title;
            }

            return null;
        }
    }
}