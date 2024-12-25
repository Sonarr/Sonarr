import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchUpdateLogFiles } from 'Store/Actions/systemActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import LogFiles from '../LogFiles';

function UpdateLogFiles() {
  const dispatch = useDispatch();
  const { isFetching, items } = useSelector(
    (state: AppState) => state.system.updateLogFiles
  );

  const isDeleteFilesExecuting = useSelector(
    createCommandExecutingSelector(commandNames.DELETE_UPDATE_LOG_FILES)
  );

  const handleRefreshPress = useCallback(() => {
    dispatch(fetchUpdateLogFiles());
  }, [dispatch]);

  const handleDeleteFilesPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.DELETE_UPDATE_LOG_FILES,
        commandFinished: () => {
          dispatch(fetchUpdateLogFiles());
        },
      })
    );
  }, [dispatch]);

  useEffect(() => {
    dispatch(fetchUpdateLogFiles());
  }, [dispatch]);

  return (
    <LogFiles
      isDeleteFilesExecuting={isDeleteFilesExecuting}
      isFetching={isFetching}
      items={items}
      type="update"
      onRefreshPress={handleRefreshPress}
      onDeleteFilesPress={handleDeleteFilesPress}
    />
  );
}

export default UpdateLogFiles;
