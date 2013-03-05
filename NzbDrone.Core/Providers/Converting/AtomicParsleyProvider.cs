using System;
using System.Diagnostics;
using System.IO;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Converting
{
    public class AtomicParsleyProvider
    {
        private readonly IConfigService _configService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AtomicParsleyProvider(IConfigService configService)
        {
            _configService = configService;
        }

        public AtomicParsleyProvider()
        {

        }

        public virtual bool RunAtomicParsley(Episode episode, string outputFile)
        {
            throw new NotImplementedException();

            var atomicParsleyLocation = _configService.GetValue("AtomicParsleyLocation", "");
            var atomicParsleyTitleType = (AtomicParsleyTitleType) Convert.ToInt32(_configService.GetValue("AtomicParsley", 0));

            var atomicParsleyCommand = String.Format("\"{0}\" --overWrite --title \"{1}\" --genre \"TV Shows\" --stik \"TV Show\" --TVShowName \"{2}\" --TVEpisodeNum \"{3}\" --TVSeason \"{4}\"",
                                        outputFile, episode.Title, episode.Series.Title, episode.EpisodeNumber, episode.SeasonNumber);

            //If Episode Number + Name should be in Episode Title (Number - Title)
            if (atomicParsleyTitleType == AtomicParsleyTitleType.EpisodeNumber)
            {
                atomicParsleyCommand = String.Format("\"{0}\" --overWrite --title \"{3} - {1}\" --genre \"TV Shows\" --stik \"TV Show\" --TVShowName \"{2}\" --TVEpisodeNum \"{3}\" --TVSeason \"{4}\"",
                                        outputFile, episode.Title, episode.Series.Title, episode.EpisodeNumber, episode.SeasonNumber);
            }

            //If Season/Episode Number + Name should be in Episode Title (SeasonNumber'x'EpisodeNumber - Title)
            else if (atomicParsleyTitleType == AtomicParsleyTitleType.Both)
            {
                atomicParsleyCommand = String.Format("\"{0}\" --overWrite --title \"{4}x{3:00} - {1}\" --genre \"TV Shows\" --stik \"TV Show\" --TVShowName \"{2}\" --TVEpisodeNum \"{3}\" --TVSeason \"{4}\"",
                                        outputFile, episode.Title, episode.Series.Title, episode.EpisodeNumber, episode.SeasonNumber);
            }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = Path.Combine(atomicParsleyLocation, "AtomicParsley.exe");
                process.StartInfo.Arguments = atomicParsleyCommand;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                //process.OutputDataReceived += new DataReceivedEventHandler(HandBrakeOutputDataReceived);
                process.Start();
                //process.BeginOutputReadLine();
                process.WaitForExit();
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
                return false;
            }

            return true;
        }
    }
}
