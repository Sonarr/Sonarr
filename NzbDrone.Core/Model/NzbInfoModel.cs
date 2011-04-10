using System;

namespace NzbDrone.Core.Model
{
    public class NzbInfoModel
    {
        public string Title { get; set; }
        public Uri Link { get; set; }

        public bool IsPassworded()
        {
            return Title.EndsWith("(Passworded)", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}