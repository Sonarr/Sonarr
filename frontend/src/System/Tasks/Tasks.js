import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import ScheduledTasksConnector from './Scheduled/ScheduledTasksConnector';
import QueuedTasksConnector from './Queued/QueuedTasksConnector';

function Tasks() {
  return (
    <PageContent title="Tasks">
      <PageContentBodyConnector>
        <ScheduledTasksConnector />
        <QueuedTasksConnector />
      </PageContentBodyConnector>
    </PageContent>
  );
}

export default Tasks;
