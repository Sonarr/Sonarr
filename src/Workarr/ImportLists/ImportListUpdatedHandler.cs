using Workarr.Messaging.Commands;
using Workarr.Messaging.Events;
using Workarr.ThingiProvider.Events;

namespace Workarr.ImportLists
{
    public class ImportListUpdatedHandler : IHandle<ProviderUpdatedEvent<IImportList>>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public ImportListUpdatedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(ProviderUpdatedEvent<IImportList> message)
        {
            _commandQueueManager.Push(new ImportListSyncCommand(message.Definition.Id));
        }
    }
}
