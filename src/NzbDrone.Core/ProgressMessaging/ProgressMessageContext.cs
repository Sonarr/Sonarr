using System;
using System.Threading;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.ProgressMessaging
{
    public static class ProgressMessageContext
    {
        private static AsyncLocal<CommandModel> _commandModelAsync = new AsyncLocal<CommandModel>();

        [ThreadStatic]
        private static CommandModel _commandModel;

        [ThreadStatic]
        private static bool _reentrancyLock;

        public static CommandModel CommandModel
        {
            get
            {
                return _commandModel ?? _commandModelAsync.Value;
            }
            set
            {
                _commandModel = value;
                _commandModelAsync.Value = value;
            }
        }

        public static bool LockReentrancy()
        {
            if (_reentrancyLock)
            {
                return false;
            }

            _reentrancyLock = true;
            return true;
        }

        public static void UnlockReentrancy()
        {
            _reentrancyLock = false;
        }
    }
}
