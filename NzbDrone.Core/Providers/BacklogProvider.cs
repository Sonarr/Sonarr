using System;

namespace NzbDrone.Core.Providers
{
    public class BacklogProvider
    {
        //Will provide Backlog Search functionality

        public virtual bool StartSearch()
        {
            throw new NotImplementedException();
        }

        public virtual bool StartSearch(int seriesId)
        {
            throw new NotImplementedException();
        }
    }
}