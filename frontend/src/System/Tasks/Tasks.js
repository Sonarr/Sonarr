import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import ScheduledTasksConnector from './Scheduled/ScheduledTasksConnector';
import QueuedTasksConnector from './Queued/QueuedTasksConnector';

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
