using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Ionic.Zip;
using Ionic.Zlib;
using NLog;
using NLog.Config;

namespace NzbDrone.Tvdb.Offline
{

    public class TVDbService
    {
        public List<string> GetSeriesId()
        {
            var startYear = 1900;

            var xml = new WebClient().DownloadString("http://www.thetvdb.com/api/Updates.php?type=all&time=" + startYear);


            var Ids = XElement.Load("http://www.thetvdb.com/api/Updates.php?type=all&time=1990")
                .Descendants("Items").Select(i=>i.Elements(""))



        }
    }



    public class Program
    {
        static readonly Logger _logger = LogManager.GetLogger("Main");
        private static DirectoryInfo _target;
        private static DirectoryInfo _temp;
        private static bool _cleanDb;
        private static string dbPath;

        static void Main(string[] args)
        {
            SetupLogger();
            _logger.Info("Starting TVDB Offline...");

            if (!String.IsNullOrWhiteSpace(dbPath))
            {
                using (var zip = new ZipFile())
                {
                    _logger.Info("Compressing database file");
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddFiles(new[] { dbPath });
                    zip.Save(dbPath + ".zip");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static void CreateNewDb()
        {
            _logger.Info("Starting to generate offline DB...");
            var files = _target.GetFiles("*.zip");
            _logger.Info("Total number of files found {0}", files.Count());

            var list = new Dictionary<int, Series>();

            var repo = InitSubsonic(true);
            decimal progress = 0;
            foreach (var fileInfo in files)
            {
                Console.Write("\r{0:0.0}%", progress * 100 / files.Count());
                var series = ProcessFile(fileInfo, repo);
                if (series != null)
                {
                    if (!list.ContainsKey(series.SeriesId))
                    {
                        list.Add(series.SeriesId, series);
                    }
                    else
                    {
                        Console.WriteLine();
                        _logger.Warn("Conflict {0} <=> {1}", list[series.SeriesId], series);
                    }
                }
                progress++;
            }

            _logger.Info("Writing series to DB");
            repo.AddMany(list.Values);
            _logger.Info("DB is fully created");


        }

        private static Series ProcessFile(FileInfo fileInfo, IRepository repo)
        {
            try
            {


                _logger.Debug("Processing " + fileInfo.Name);
                using (ZipFile zip = ZipFile.Read(fileInfo.FullName))
                {

                    ZipEntry e = zip["en.xml"];
                    if (e == null)
                    {
                        _logger.Warn("File {0} didn't contain an en.xml file", fileInfo.Name);
                        return null;
                    }

                    var stream = e.OpenReader();
                    var seriesElement = XDocument.Load(stream).Descendants("Series").First();

                    var series = new Series();
                    series.SeriesId = (int)seriesElement.Element("id");

                    series.AirsDayOfWeek = seriesElement.Element("Airs_DayOfWeek").Value;
                    series.AirTimes = seriesElement.Element("Airs_Time").Value;
                    series.Overview = seriesElement.Element("Overview").Value;
                    series.Status = seriesElement.Element("Status").Value;
                    series.Title = seriesElement.Element("SeriesName").Value;

                    int ratingCount;
                    Int32.TryParse(seriesElement.Element("RatingCount").Value, out ratingCount);
                    series.RateCount = ratingCount;

                    decimal rating;
                    Decimal.TryParse(seriesElement.Element("Rating").Value, out rating);
                    series.RateCount = ratingCount;

                    series.CleanTitle = Core.Parser.NormalizeTitle(series.Title);
                    series.Path = fileInfo.Name;

                    return series;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Unable to process file. {0}. {1}", fileInfo.Name, e.Message);
                return null;
            }


        }


        private static void SetupLogger()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config", false);
        }
    }
}
