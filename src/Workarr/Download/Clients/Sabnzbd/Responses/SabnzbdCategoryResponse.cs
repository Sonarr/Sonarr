﻿namespace Workarr.Download.Clients.Sabnzbd.Responses
{
    public class SabnzbdCategoryResponse
    {
        public SabnzbdCategoryResponse()
        {
            Categories = new List<string>();
        }

        public List<string> Categories { get; set; }
    }
}
