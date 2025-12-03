import React, { useCallback } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import LogFiles from '../LogFiles';
import { useUpdateLogFiles } from '../useLogFiles';

function UpdateLogFiles() {
  const executeCommand = useExecuteCommand();
  const { data = [], isFetching, refetch } = useUpdateLogFiles();

  const isDeleteFilesExecuting = useCommandExecuting(
    CommandNames.DeleteUpdateLogFiles
  );

  const handleRefreshPress = useCallback(() => {
    refetch();
  }, [refetch]);

  const handleDeleteFilesPress = useCallback(() => {
    executeCommand(
      {
        name: CommandNames.DeleteUpdateLogFiles,
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
      type="update"
      onRefreshPress={handleRefreshPress}
      onDeleteFilesPress={handleDeleteFilesPress}
    />
  );
}

export default UpdateLogFiles;
