using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationBase<TSettings> : INotification
        where TSettings : NotificationSettingsBase<TSettings>, new()
    {
        protected const string EPISODE_GRABBED_TITLE = "Episode Grabbed";
        protected const string EPISODE_DOWNLOADED_TITLE = "Episode Downloaded";
        protected const string IMPORT_COMPLETE_TITLE = "Import Complete";
        protected const string EPISODE_DELETED_TITLE = "Episode Deleted";
        protected const string SERIES_ADDED_TITLE = "Series Added";
        protected const string SERIES_DELETED_TITLE = "Series Deleted";
        protected const string HEALTH_ISSUE_TITLE = "Health Check Failure";
        protected const string HEALTH_RESTORED_TITLE = "Health Check Restored";
        protected const string APPLICATION_UPDATE_TITLE = "Application Updated";
        protected const string MANUAL_INTERACTION_REQUIRED_TITLE = "Manual Interaction";

        protected const string EPISODE_GRABBED_TITLE_BRANDED = "Sonarr - " + EPISODE_GRABBED_TITLE;
        protected const string EPISODE_DOWNLOADED_TITLE_BRANDED = "Sonarr - " + EPISODE_DOWNLOADED_TITLE;
        protected const string IMPORT_COMPLETE_TITLE_BRANDED = "Sonarr - " + IMPORT_COMPLETE_TITLE;
        protected const string EPISODE_DELETED_TITLE_BRANDED = "Sonarr - " + EPISODE_DELETED_TITLE;
        protected const string SERIES_ADDED_TITLE_BRANDED = "Sonarr - " + SERIES_ADDED_TITLE;
        protected const string SERIES_DELETED_TITLE_BRANDED = "Sonarr - " + SERIES_DELETED_TITLE;
        protected const string HEALTH_ISSUE_TITLE_BRANDED = "Sonarr - " + HEALTH_ISSUE_TITLE;
        protected const string HEALTH_RESTORED_TITLE_BRANDED = "Sonarr - " + HEALTH_RESTORED_TITLE;
        protected const string APPLICATION_UPDATE_TITLE_BRANDED = "Sonarr - " + APPLICATION_UPDATE_TITLE;
        protected const string MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED = "Sonarr - " + MANUAL_INTERACTION_REQUIRED_TITLE;

        public abstract string Name { get; }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();

        public ProviderDefinition Definition { get; set; }
        public abstract ValidationResult Test();

        public abstract string Link { get; }

        public virtual void OnGrab(GrabMessage grabMessage)
        {
        }

        public virtual void OnDownload(DownloadMessage message)
        {
        }

        public virtual void OnImportComplete(ImportCompleteMessage message)
        {
        }

        public virtual void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
        }

        public virtual void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
        }

        public virtual void OnSeriesAdd(SeriesAddMessage message)
        {
        }

        public virtual void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
        }

        public virtual void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
        }

        public virtual void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
        }

        public virtual void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
        }

        public virtual void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
        }

        public virtual void ProcessQueue()
        {
        }

        public bool SupportsOnGrab => HasConcreteImplementation("OnGrab");
        public bool SupportsOnRename => HasConcreteImplementation("OnRename");
        public bool SupportsOnDownload => HasConcreteImplementation("OnDownload");
        public bool SupportsOnUpgrade => SupportsOnDownload;
        public bool SupportsOnImportComplete => HasConcreteImplementation("OnImportComplete");
        public bool SupportsOnSeriesAdd => HasConcreteImplementation("OnSeriesAdd");
        public bool SupportsOnSeriesDelete => HasConcreteImplementation("OnSeriesDelete");
        public bool SupportsOnEpisodeFileDelete => HasConcreteImplementation("OnEpisodeFileDelete");
        public bool SupportsOnEpisodeFileDeleteForUpgrade => SupportsOnEpisodeFileDelete;
        public bool SupportsOnHealthIssue => HasConcreteImplementation("OnHealthIssue");
        public bool SupportsOnHealthRestored => HasConcreteImplementation("OnHealthRestored");
        public bool SupportsOnApplicationUpdate => HasConcreteImplementation("OnApplicationUpdate");
        public bool SupportsOnManualInteractionRequired => HasConcreteImplementation("OnManualInteractionRequired");

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        private bool HasConcreteImplementation(string methodName)
        {
            var method = GetType().GetMethod(methodName);

            if (method == null)
            {
                throw new MissingMethodException(GetType().Name, Name);
            }

            return !method.DeclaringType.IsAbstract;
        }
    }
}
