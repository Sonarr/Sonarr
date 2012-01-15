using System;
using System.Linq;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs
{
    public interface IJob
    {
        /// <summary>
        /// Name of the timer.
        /// This is the name that will be visible in all UI elements
        /// </summary>\\\
        string Name { get; }


        /// <summary>
        /// Default Interval that this job should run at. In seconds.
        /// </summary>
        /// <remarks>Setting this value to 0 means the job will not be 
        /// executed by the schedule and is only triggered manually.</remarks>
        TimeSpan DefaultInterval { get; }


        /// <summary>
        /// Starts the job
        /// </summary>
        /// <param name="notification">Notification object that is passed in by JobProvider.
        /// this object should be used to update the progress on the UI</param>
        /// <param name="targetId">The that should be used to limit the target of this job</param>
        /// /// <param name="secondaryTargetId">The that should be used to limit the target of this job</param>
        void Start(ProgressNotification notification, int targetId, int secondaryTargetId);
    }
}