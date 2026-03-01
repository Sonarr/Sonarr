import React, { useEffect, useMemo } from 'react';
import { useAppValues } from 'App/appStore';
import PageSidebarStatus from 'Components/Page/Sidebar/PageSidebarStatus';
import usePrevious from 'Helpers/Hooks/usePrevious';
import translate from 'Utilities/String/translate';
import useHealth from './useHealth';

function HealthStatus() {
  const { isConnected, isReconnecting } = useAppValues(
    'isConnected',
    'isReconnecting'
  );
  const { data, refetch } = useHealth();

  const wasReconnecting = usePrevious(isReconnecting);

  const { count, errors, warnings } = useMemo(() => {
    let errors = false;
    let warnings = false;

    data.forEach((item) => {
      if (item.type === 'error') {
        errors = true;
      }

      if (item.type === 'warning') {
        warnings = true;
      }
    });

    return {
      count: data.length,
      errors,
      warnings,
    };
  }, [data]);

  useEffect(() => {
    if (isConnected && wasReconnecting) {
      refetch();
    }
  }, [isConnected, wasReconnecting, refetch]);

  return (
    <PageSidebarStatus
      aria-label={
        count === 1
          ? translate('HealthIssue')
          : translate('HealthIssues', { count })
      }
      count={count}
      errors={errors}
      warnings={warnings}
    />
  );
}

export default HealthStatus;
