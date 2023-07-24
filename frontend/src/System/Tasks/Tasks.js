import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import translate from 'Utilities/String/translate';
import QueuedTasksConnector from './Queued/QueuedTasksConnector';
import ScheduledTasksConnector from './Scheduled/ScheduledTasksConnector';

function Tasks() {
  return (
    <PageContent title={translate('Tasks')}>
      <PageContentBody>
        <ScheduledTasksConnector />
        <QueuedTasksConnector />
      </PageContentBody>
    </PageContent>
  );
}

export default Tasks;
