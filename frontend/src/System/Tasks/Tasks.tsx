import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import translate from 'Utilities/String/translate';
import QueuedTasks from './Queued/QueuedTasks';
import ScheduledTasks from './Scheduled/ScheduledTasks';

function Tasks() {
  return (
    <PageContent title={translate('Tasks')}>
      <PageContentBody>
        <PageHeading scope={translate('System')} title={translate('Tasks')} />
        <ScheduledTasks />
        <QueuedTasks />
      </PageContentBody>
    </PageContent>
  );
}

export default Tasks;
