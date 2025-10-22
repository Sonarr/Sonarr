import React, { useEffect, useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import PageSidebarStatus from 'Components/Page/Sidebar/PageSidebarStatus';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useHealth from './useHealth';

function HealthStatus() {
  const { isConnected, isReconnecting } = useSelector(
    (state: AppState) => state.app
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
    <PageSidebarStatus count={count} errors={errors} warnings={warnings} />
  );
}

export default HealthStatus;
