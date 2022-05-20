import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import QueuedTasksConnector from './Queued/QueuedTasksConnector';
import ScheduledTasksConnector from './Scheduled/ScheduledTasksConnector';

function Tasks() {
  return (
    <PageContent title="Tasks">
      <PageContentBody>
        <ScheduledTasksConnector />
        <QueuedTasksConnector />
      </PageContentBody>
    </PageContent>
  );
}

export default Tasks;
