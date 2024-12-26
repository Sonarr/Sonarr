using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Notifications.NotificationTemplates
{
    public class NotificationTemplate : ModelBase, IEquatable<NotificationTemplate>
    {
        public NotificationTemplate()
        {
        }

        public NotificationTemplate(
            string name,
            string title,
            string body,
            bool onGrab,
            bool onDownload,
            bool onUpgrade,
            bool onImportComplete,
            bool onRename,
            bool onSeriesAdd,
            bool onSeriesDelete,
            bool onEpisodeFileDelete,
            bool onEpisodeFileDeleteForUpgrade,
            bool onHealthIssue,
            bool onHealthRestored,
            bool onApplicationUpdate,
            bool onManualInteractionRequired)
        {
            Name = name;
            Title = title;
            Body = body;
            OnGrab = onGrab;
            OnDownload = onDownload;
            OnUpgrade = onUpgrade;
            OnImportComplete = onImportComplete;
            OnRename = onRename;
            OnSeriesAdd = onSeriesAdd;
            OnSeriesDelete = onSeriesDelete;
            OnEpisodeFileDelete = onEpisodeFileDelete;
            OnEpisodeFileDeleteForUpgrade = onEpisodeFileDeleteForUpgrade;
            OnHealthIssue = onHealthIssue;
            OnHealthRestored = onHealthRestored;
            OnApplicationUpdate = onApplicationUpdate;
            OnManualInteractionRequired = onManualInteractionRequired;
        }

        public string Name { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnImportComplete { get; set; }
        public bool OnRename { get; set; }
        public bool OnSeriesAdd { get; set; }
        public bool OnSeriesDelete { get; set; }
        public bool OnEpisodeFileDelete { get; set; }
        public bool OnEpisodeFileDeleteForUpgrade { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool OnHealthRestored { get; set; }
        public bool OnApplicationUpdate { get; set; }
        public bool OnManualInteractionRequired { get; set; }
        public bool Equals(NotificationTemplate other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((NotificationTemplate)obj);
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
