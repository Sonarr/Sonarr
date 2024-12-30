using Newtonsoft.Json;
using Workarr.Serializer.System.Text.Json;

namespace Workarr.Messaging.Commands
{
    [JsonConverter(typeof(PolymorphicWriteOnlyJsonConverter<Command>))]
    public abstract class Command
    {
        private bool _sendUpdatesToClient;

        public virtual bool SendUpdatesToClient
        {
            get
            {
                return _sendUpdatesToClient;
            }

            set
            {
                _sendUpdatesToClient = value;
            }
        }

        public virtual bool UpdateScheduledTask => true;
        public virtual string CompletionMessage => null;
        public virtual bool RequiresDiskAccess => false;
        public virtual bool IsExclusive => false;
        public virtual bool IsLongRunning => false;

        public string Name { get; private set; }
        public DateTime? LastExecutionTime { get; set; }
        public DateTime? LastStartTime { get; set; }
        public CommandTrigger Trigger { get; set; }
        public bool SuppressMessages { get; set; }

        public string ClientUserAgent { get; set; }

        public Command()
        {
            Name = GetType().Name.Replace("Command", "");
        }
    }
}
