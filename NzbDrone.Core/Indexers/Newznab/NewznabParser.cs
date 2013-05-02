using System;
using System.ServiceModel.Syndication;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabParser : BasicRssParser
    {
        private readonly Newznab _newznabIndexer;

        public NewznabParser(Newznab newznabIndexer)
        {
            _newznabIndexer = newznabIndexer;
        }

        protected override string GetNzbInfoUrl(SyndicationItem item)
        {
            return item.Id;
        }

        protected override ReportInfo PostProcessor(SyndicationItem item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                if (item.Links.Count > 1)
                {
                    currentResult.Size = item.Links[1].Length;
                }

                currentResult.Indexer = GetName(item);
            }

            return currentResult;
        }


        private string GetName(SyndicationItem item)
        {
            var hostname = item.Links[0].Uri.DnsSafeHost.ToLower();
            return String.Format("{0}_{1}", _newznabIndexer.Name, hostname);
        }
    }
}