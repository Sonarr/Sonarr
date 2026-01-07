import React from 'react';
import PageSidebarStatus from 'Components/Page/Sidebar/PageSidebarStatus';
import translate from 'Utilities/String/translate';
import useQueueStatus from './useQueueStatus';

function QueueStatus() {
  const { errors, warnings, count } = useQueueStatus();

  return (
    <PageSidebarStatus
      aria-label={
        count === 1
          ? translate('QueueItem')
          : translate('QueueItems', { count })
      }
      count={count}
      errors={errors}
      warnings={warnings}
    />
  );
}

export default QueueStatus;
