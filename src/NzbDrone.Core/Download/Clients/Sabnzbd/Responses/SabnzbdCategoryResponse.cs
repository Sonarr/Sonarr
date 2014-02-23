using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Sabnzbd.Responses
{
    public class SabnzbdCategoryResponse
    {
        public SabnzbdCategoryResponse()
        {
            Categories = new List<String>();
        }

        public List<String> Categories { get; set; }
    }
}
