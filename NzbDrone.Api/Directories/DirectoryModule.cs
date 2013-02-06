using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Common;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Api.Directories
{
    public class DirectoryModule : NzbDroneApiModule
    {
        private readonly DiskProvider _diskProvider;

        public DirectoryModule(DiskProvider diskProvider)
            : base("/directories")
        {
            _diskProvider = diskProvider;
            Post["/"] = x => GetDirectories();
        }

        private Response GetDirectories()
        {
            if (!Request.Form.query.HasValue)
                return new List<string>().AsResponse();

            string query = Request.Form.query.Value;

            IEnumerable<String> dirs = null;
            try
            {
                //Windows (Including UNC)
                var windowsSep = query.LastIndexOf('\\');

                if (windowsSep > -1)
                {
                    dirs = _diskProvider.GetDirectories(query.Substring(0, windowsSep + 1));
                }

                //Unix
                var index = query.LastIndexOf('/');

                if (index > -1)
                {
                    dirs = _diskProvider.GetDirectories(query.Substring(0, index + 1));
                }
            }
            catch (Exception)
            {
                //Swallow the exceptions so proper JSON is returned to the client (Empty results)
                return new List<string>().AsResponse();
            }

            if (dirs == null)
                throw new Exception("A valid path was not provided");

            return dirs.AsResponse();
        }
    }
}