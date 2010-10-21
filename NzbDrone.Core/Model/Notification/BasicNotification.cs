using System;

namespace NzbDrone.Core.Model.Notification
{
    public class BasicNotification
    {
        public BasicNotification()
        {
            Id = Guid.Empty;
        }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        /// <value>The Id.</value>
        public Guid Id { get; private set; }

        public String Title { get; set; }

        public BasicNotificationType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this message should be automatically dismissed after a period of time.
        /// </summary>
        /// <value><c>true</c> if [auto dismiss]; otherwise, <c>false</c>.</value>
        public bool AutoDismiss { get; set; }
    }
}