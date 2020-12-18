﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationBase<TSettings> : INotification where TSettings : IProviderConfig, new()
    {
        protected const string EPISODE_GRABBED_TITLE = "Episode Grabbed";
        protected const string EPISODE_DOWNLOADED_TITLE = "Episode Downloaded";
        protected const string EPISODE_DELETED_TITLE = "Episode Deleted";
        protected const string SERIES_DELETED_TITLE = "Series Deleted";
        protected const string HEALTH_ISSUE_TITLE = "Health Check Failure";

        protected const string EPISODE_GRABBED_TITLE_BRANDED = "Sonarr - " + EPISODE_GRABBED_TITLE;
        protected const string EPISODE_DOWNLOADED_TITLE_BRANDED = "Sonarr - " + EPISODE_DOWNLOADED_TITLE;
        protected const string EPISODE_DELETED_TITLE_BRANDED = "Sonarr - " + EPISODE_DELETED_TITLE;
        protected const string SERIES_DELETED_TITLE_BRANDED = "Sonarr - " + SERIES_DELETED_TITLE;
        protected const string HEALTH_ISSUE_TITLE_BRANDED = "Sonarr - " + HEALTH_ISSUE_TITLE;
        
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

        public virtual void OnRename(Series series)
        {

        }

        public virtual void OnDelete(EpisodeDeleteMessage deleteMessage)
        {

        }

        public virtual void OnDelete(SeriesDeleteMessage deleteMessage)
        {

        }

        public virtual void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {

        }

        public virtual void ProcessQueue()
        {

        }

        public bool SupportsOnGrab => HasConcreteImplementation("OnGrab");
        public bool SupportsOnRename => HasConcreteImplementation("OnRename");
        public bool SupportsOnDownload => HasConcreteImplementation("OnDownload");
        public bool SupportsOnUpgrade => SupportsOnDownload;
        public bool SupportsOnDelete => HasConcreteImplementation("OnDelete");
        public bool SupportsOnHealthIssue => HasConcreteImplementation("OnHealthIssue");

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }

        public virtual object RequestAction(string action, IDictionary<string, string> query) { return null; }


        private bool HasConcreteImplementation(string methodName)
        {
            if (methodName != "OnDelete")
            {
                var method = GetType().GetMethod(methodName);

                if (method == null)
                {
                    throw new MissingMethodException(GetType().Name, Name);
                }

                return !method.DeclaringType.IsAbstract;
            }
            else
            {
                var methods = GetType().GetMethods().Where(method => method.Name == methodName);

                if (methods == null)
                {
                    throw new MissingMethodException(GetType().Name, Name);
                }

                var methodsArray = methods.ToArray();

                if (methodsArray.Count() != 2)
                {
                    return false;
                }
                else
                {
                    return !methodsArray[0].DeclaringType.IsAbstract;
                }
            }
        }
    }
}
