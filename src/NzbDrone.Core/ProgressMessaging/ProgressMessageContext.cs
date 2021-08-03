using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.ProgressMessaging
{
    public static class ProgressMessageContext
    {
        [ThreadStatic]
        private static CommandModel _commandModel;

        [ThreadStatic]
        private static bool _reentrancyLock;

        public static CommandModel CommandModel
        {
            get { return _commandModel; }
            set { _commandModel = value; }
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
