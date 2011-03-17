using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public interface IBacklogProvider
    {
        //Will provide Backlog Search functionality

        bool StartSearch();
        bool StartSearch(int seriesId);
    }
}
