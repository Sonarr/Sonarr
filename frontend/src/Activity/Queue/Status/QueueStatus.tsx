import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import PageSidebarStatus from 'Components/Page/Sidebar/PageSidebarStatus';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { fetchQueueStatus } from 'Store/Actions/queueActions';
import createQueueStatusSelector from './createQueueStatusSelector';

function QueueStatus() {
  const dispatch = useDispatch();
  const { isConnected, isReconnecting } = useSelector(
    (state: AppState) => state.app
  );
  const { isPopulated, count, errors, warnings } = useSelector(
    createQueueStatusSelector()
  );

  const wasReconnecting = usePrevious(isReconnecting);

  useEffect(() => {
    if (!isPopulated) {
      dispatch(fetchQueueStatus());
    }
  }, [isPopulated, dispatch]);

  useEffect(() => {
    if (isConnected && wasReconnecting) {
      dispatch(fetchQueueStatus());
    }
  }, [isConnected, wasReconnecting, dispatch]);

  return (
    <PageSidebarStatus count={count} errors={errors} warnings={warnings} />
  );
}

export default QueueStatus;
