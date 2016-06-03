using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationError
    {
        private static readonly Dictionary<int, string> CommonMessages;
        private static readonly Dictionary<int, string> AuthMessages;
        private static readonly Dictionary<int, string> DownloadStationTaskMessages;

        static DownloadStationError()
        {
            CommonMessages = new Dictionary<int, string>
            {
                {100, "Unknown error"},
                {101, "Invalid parameter"},
                {102, "The requested API does not exist"},
                {103, "The requested method does not exist"},
                {104, "The requested version does not support the functionality"},
                {105, "The logged in session does not have permission"},
                {106, "Session timeout"},
                {107, "Session interrupted by duplicate login"}
            };

            AuthMessages = new Dictionary<int, string>
            {
                {400, "No such account or incorrect password"},
                {401, "Account disabled"},
                {402, "Permission denied"},
                {403, "2-step verification code required"},
                {404, "Failed to authenticate 2-step verification code"}
            };

            DownloadStationTaskMessages = new Dictionary<int, string>
            {
                {400, "File upload failed"},
                {401, "Max number of tasks reached"},
                {402, "Destination denied"},
                {403, "Destination does not exist"},
                {404, "Invalid task id"},
                {405, "Invalid task action"},
                {406, "No default destination"},
                {407, "Set destination failed"},
                {408, "File does not exist"}
            };
        }

        public int Code { get; set; }

        public bool SessionError
        {
            get
            {
                return Code == 105 || Code == 106 || Code == 107;
            }
        }

        public string GetMessage(SynologyApi api)
        {
            if (api == SynologyApi.Auth)
            {
                if (AuthMessages.ContainsKey(Code))
                {
                    return AuthMessages[Code];
                }
            }
            else if (api == SynologyApi.DownloadStationTask && DownloadStationTaskMessages.ContainsKey(Code))
            {
                return DownloadStationTaskMessages[Code];
            }

            return CommonMessages[Code];
        }
    }
}
