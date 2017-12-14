using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Timeline
{
    public interface IProgressReporter
    {
        long Raw { get; }
        long Total { get; }
        double Progress { get; }

        void UpdateProgress(long currentProgress, long maxProgress);
        void FinishProgress();
    }

    public class ProgressReporter : IProgressReporter
    {
        private readonly int _maxSteps;

        public long Raw { get; protected set; }
        public long Total { get; private set; }

        public double Progress => Total == 0 ? 1.0 : Math.Min(Raw, Total) / Total;

        //public TimeSpan? EstimatedDurationRemaining { get; private set; }

        public ProgressReporter(long initialProgress, long maxProgress, int maxSteps = 100)
        {
            _maxSteps = maxSteps;

            Raw = initialProgress;
            Total = maxProgress;
        }

        public void UpdateProgress(long currentProgress, long maxProgress)
        {
            bool shouldRaiseEvent;

            lock (this)
            {
                var oldRaw = Raw;
                var oldTotal = Total;

                Raw = currentProgress;
                Total = Total;

                var oldStep = oldTotal <= 0 ? _maxSteps : oldRaw * _maxSteps / oldTotal;
                var newStep = Total <= 0 ? _maxSteps : Raw * _maxSteps / Total;

                shouldRaiseEvent = (oldStep != newStep);
            }

            if (shouldRaiseEvent)
            {
                RaiseEvent();
            }
        }

        public void FinishProgress()
        {
            lock (this)
            {
                Raw = Total;
            }

            RaiseEvent();
        }

        protected virtual void RaiseEvent()
        {
            // TODO
        }

    }
}
