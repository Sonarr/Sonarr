using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Ionic.Zip;
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


        static void Main(string[] args)
        {
            SetupLogger();
            _logger.Info("Starting TVDB Offline...");
            GetPath(args);

            Start();


            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }


        private static void Start()
        {
            _logger.Info("Starting to generate offline DB...");
            var files = _target.GetFiles("*.zip");
            _logger.Info("Total number of files found {0}", files.Count());

            var list = new Dictionary<int, Series>();

            var repo = InitSubsonic();
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

        private static IRepository InitSubsonic()
        {
            var path = Path.Combine(_temp.FullName, "series_data.db");
            _logger.Info("Creating Database file at {0}", path);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            string logConnectionString = String.Format("Data Source={0};Version=3;", path);
            var provider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SQLite");

            return new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations);
        }

        private static void GetPath(string[] args)
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
    }
}
