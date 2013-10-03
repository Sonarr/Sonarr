using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Update
{
    public class UpdateChanges
    {
        public List<String> New { get; set; }
        public List<String> Fixed { get; set; }

        public UpdateChanges()
        {
            New = new List<String>();
            Fixed = new List<String>();
        }
    }
}
