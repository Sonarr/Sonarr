using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.ProgressMessaging
{
    public class ProgressMessageModule : NzbDroneRestModule<ProgressMessageResource>
    {
        public ProgressMessageModule()
        {
            Get["/"] = x => GetAllMessages();
        }

        private Response GetAllMessages()
        {
            return new List<ProgressMessageResource>().AsResponse();
        }
    }
}