using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationError
    {
        private static readonly Dictionary<int, string> CommonMessages;
        private static readonly Dictionary<int, string> AuthenticationMessages;
        private static readonly Dictionary<int, string> TaskMessages;

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

            AuthenticationMessages = new Dictionary<int, string>
            {
                {400, "No such account or incorrect password"},
                {401, "Account disabled"},
                {402, "Permission denied"},
                {403, "2-step verification code required"},
                {404, "Failed to authenticate 2-step verification code"}
            };

            TaskMessages = new Dictionary<int, string>
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

        public string GetMessage(DownloadStationRequestApi api)
        {
            if (api == DownloadStationRequestApi.Authentication)
            {
                if (AuthenticationMessages.ContainsKey(Code))
                {
                    return AuthenticationMessages[Code];
                }
            }
            else if (api == DownloadStationRequestApi.Task && TaskMessages.ContainsKey(Code))
            {
                return TaskMessages[Code];
            }

            return CommonMessages[Code];
        }
    }
}
