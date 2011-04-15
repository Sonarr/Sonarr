using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Ionic.Zip;
using Ionic.Zlib;
using NLog;
using NLog.Config;
using SubSonic.DataProviders;
using SubSonic.Repository;

namespace NzbDrone.Tvdb.Offline
{
    class Program
    {
        static Logger _logger = LogManager.GetLogger("Main");
        private static DirectoryInfo _target;
        private static DirectoryInfo _temp;
        private static bool _cleanDb;
        private static string dbPath;

        static void Main(string[] args)
        {
            SetupLogger();
            _logger.Info("Starting TVDB Offline...");
            ProcessArguments(args);

            if (_cleanDb)
            {
                CleanUpDb();
            }
            else
            {
                CreateNewDb();
            }


            if (!String.IsNullOrWhiteSpace(dbPath))
            {
                using (ZipFile zip = new ZipFile())
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

        private static IRepository InitSubsonic(bool purge, string name = "")
        {
            dbPath = Path.Combine(_temp.FullName, "series_data" + name + ".db");
            _logger.Info("Loading Database file at {0}", dbPath);

            if (purge && File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }

            string logConnectionString = String.Format("Data Source={0};Version=3;", dbPath);
            var provider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SQLite");

            return new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations);
        }

        private static void ProcessArguments(string[] args)
        {
            if (args == null || args.Count() == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                _logger.Warn("Please provide a valid target path");
                Environment.Exit(0);
            }

            _target = new DirectoryInfo(args[0]);

            if (!_target.Exists)
            {
                _logger.Warn("Directory '{0}' doesn't exist.", _target.FullName);
                Environment.Exit(0);
            }

            if (args.Count() > 1 && !string.IsNullOrWhiteSpace(args[1]) && args[1].Trim().ToLower() == "/clean")
            {
                _cleanDb = true;
            }
            _logger.Info("Target Path '[{0}]'", _target.FullName);

            _logger.Debug("Creating temporary folder");
            _temp = _target.CreateSubdirectory("temp");
        }

        private static void SetupLogger()
        {

            LogManager.ThrowExceptions = true;

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration("log.config", false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private static void CleanUpDb()
        {
            _logger.Info("Cleaning up database");
            var repo = InitSubsonic(false);
            var series = repo.All<Series>().ToList();
            var cleanSeries = new List<Series>();
            decimal progress = 0;

            foreach (var item in series)
            {
                Console.Write("\r{0:0.0}%", progress * 100 / series.Count());

                var clean = CleanSeries(item);

                if (clean != null)
                {
                    cleanSeries.Add(clean);
                }


                progress++;
            }

            repo = InitSubsonic(true, "_cleanTitle");
            _logger.Info("Writing clean list to database");
            repo.AddMany(cleanSeries);
        }


        private static Series CleanSeries(Series series)
        {
            if (String.IsNullOrWhiteSpace(series.Title))
            {
                return null;
            }

            if (String.IsNullOrWhiteSpace(series.AirsDayOfWeek))
            {
                series.AirsDayOfWeek = null;
            }
            else
            {
                //if (series.AirsDayOfWeek.ToLower() == "daily")
                //{
                //    series.WeekDay = 8;
                //}
                //else
                //{
                //    DayOfWeek weekdayEnum;
                //    if (Enum.TryParse(series.AirsDayOfWeek, true, out weekdayEnum))
                //    {
                //        series.WeekDay = (int)weekdayEnum;
                //    }
                //    else
                //    {
                //        _logger.Warn("Can't parse weekday enum " + series.AirsDayOfWeek);
                //    }
                //}
                if (String.IsNullOrWhiteSpace(series.AirsDayOfWeek))
                {
                    series.AirsDayOfWeek = null;
                }
            }

            if (String.IsNullOrWhiteSpace(series.Status))
            {
                series.Active = null;
            }
            else if (series.Status == "Ended")
            {
                series.Active = false;
            }
            else if (series.Status == "Continuing")
            {
                series.Active = true;
            }
            else
            {
                _logger.Warn("Can't parse status " + series.Status);
            }

            series.Status = null;
            series.Overview = null;
            series.CleanTitle = null;

            return series;
        }
    }
}
