using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.FreeboxDownload.Responses
{
    public class FreeboxResponse<T>
    {
        private static readonly Dictionary<string, string> Descriptions;

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "error_code")]
        public string ErrorCode { get; set; }
        [JsonProperty(PropertyName = "result")]
        public T Result { get; set; }

        static FreeboxResponse()
        {
            Descriptions = new Dictionary<string, string>
            {
                // Common errors
                { "invalid_request", "Your request is invalid." },
                { "invalid_api_version", "Invalid API base url or unknown API version." },
                { "internal_error", "Internal error." },

                // Login API errors
                { "auth_required", "Invalid session token, or no session token sent." },
                { "invalid_token", "The app token you are trying to use is invalid or has been revoked." },
                { "pending_token", "The app token you are trying to use has not been validated by user yet." },
                { "insufficient_rights", "Your app permissions does not allow accessing this API." },
                { "denied_from_external_ip", "You are trying to get an app_token from a remote IP." },
                { "ratelimited", "Too many auth error have been made from your IP." },
                { "new_apps_denied", "New application token request has been disabled." },
                { "apps_denied", "API access from apps has been disabled." },

                // Download API errors
                { "task_not_found", "No task was found with the given id." },
                { "invalid_operation", "Attempt to perform an invalid operation." },
                { "invalid_file", "Error with the download file (invalid format ?)." },
                { "invalid_url", "URL is invalid." },
                { "not_implemented", "Method not implemented." },
                { "out_of_memory", "No more memory available to perform the requested action." },
                { "invalid_task_type", "The task type is invalid." },
                { "hibernating", "The downloader is hibernating." },
                { "need_bt_stopped_done", "This action is only valid for Bittorrent task in stopped or done state." },
                { "bt_tracker_not_found", "Attempt to access an invalid tracker object." },
                { "too_many_tasks", "Too many tasks." },
                { "invalid_address", "Invalid peer address." },
                { "port_conflict", "Port conflict when setting config." },
                { "invalid_priority", "Invalid priority." },
                { "ctx_file_error", "Failed to initialize task context file (need to check disk)." },
                { "exists", "Same task already exists." },
                { "port_outside_range", "Incoming port is not available for this customer." }
            };
        }

        public string GetErrorDescription()
        {
            if (Descriptions.ContainsKey(ErrorCode))
            {
                return Descriptions[ErrorCode];
            }

            return $"{ErrorCode} - Unknown error";
        }
    }
}
