import React, { useCallback } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import LogFiles from '../LogFiles';
import useLogFiles from '../useLogFiles';

function AppLogFiles() {
  const executeCommand = useExecuteCommand();
  const { data = [], isFetching, refetch } = useLogFiles();

  const isDeleteFilesExecuting = useCommandExecuting(
    CommandNames.DeleteLogFiles
  );

  const handleRefreshPress = useCallback(() => {
    refetch();
  }, [refetch]);

  const handleDeleteFilesPress = useCallback(() => {
    executeCommand(
      {
        name: CommandNames.DeleteLogFiles,
      },
      () => {
        refetch();
      }
    );
  }, [executeCommand, refetch]);

  return (
    <LogFiles
      isDeleteFilesExecuting={isDeleteFilesExecuting}
      isFetching={isFetching}
      items={data}
      type="app"
      onRefreshPress={handleRefreshPress}
      onDeleteFilesPress={handleDeleteFilesPress}
    />
  );
}

export default AppLogFiles;
