using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Timeline
{
    public enum TimelineState
    {
        Pending,
        Started,
        Completed,
        Failed
    }

    public interface ITimelineContext : IProgressReporter
    {
        string Name { get; }
        TimelineState State { get; }

        void UpdateState(TimelineState state);
        ITimelineContext AppendTimeline(string name, long initialProgress = 0, long maxProgress = 1);
        void AppendTimeline(ITimelineContext timeline);
    }

    public class TimelineContext : ProgressReporter, ITimelineContext
    {
        private List<ITimelineContext> _timelines = new List<ITimelineContext>();

        public string Name { get; private set; }
        public TimelineState State { get; private set; }

        public IEnumerable<ITimelineContext> Timelines
        {
            get
            {
                lock (this)
                {
                    return _timelines.ToArray();
                }
            }
        }

        public TimelineContext(string name, long initialProgress, long maxProgress)
            : base(initialProgress, maxProgress)
        {
            Name = name;
        }

        public void UpdateState(TimelineState state)
        {
            lock (this)
            {
                State = state;

                if (State == TimelineState.Completed || State == TimelineState.Failed)
                {
                    Raw = Total;
                }
            }

            RaiseEvent();
        }

        public ITimelineContext AppendTimeline(string name, long initialProgress = 0, long maxProgress = 1)
        {
            lock (this)
            {
                var timeline = new TimelineContext(name, initialProgress, maxProgress);
                _timelines.Add(timeline);
                return timeline;
            }
        }

        public void AppendTimeline(ITimelineContext timeline)
        {
            lock (this)
            {
                _timelines.Add(timeline);
            }
        }

        protected override void RaiseEvent()
        {
            lock (this)
            {
                if (Raw == Total)
                {
                    State = TimelineState.Completed;
                }
                else
                {
                    State = TimelineState.Started;
                }
            }

            base.RaiseEvent();
        }
    }
}
