import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import translate from 'Utilities/String/translate';
import QueuedTasks from './Queued/QueuedTasks';
import ScheduledTasksConnector from './Scheduled/ScheduledTasksConnector';

function Tasks() {
  return (
    <PageContent title={translate('Tasks')}>
      <PageContentBody>
        <ScheduledTasksConnector />
        <QueuedTasks />
      </PageContentBody>
    </PageContent>
  );
}

export default Tasks;
