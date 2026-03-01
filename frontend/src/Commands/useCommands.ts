import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo, useRef } from 'react';
import { showMessage } from 'App/messagesStore';
import Command, { CommandBody, NewCommandBody } from 'Commands/Command';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import {
  ERROR,
  INFO,
  type MessageType,
  SUCCESS,
} from 'Helpers/Props/messageTypes';
import { isSameCommand } from 'Utilities/Command';

const DEFAULT_COMMANDS: Command[] = [];
const COMMAND_REFETCH_INTERVAL = 5 * 60 * 1000;

const commandFinishedCallbacks: Record<number, (command: Command) => void> = {};

export const useCommands = () => {
  const result = useApiQuery<Command[]>({
    path: '/command',
    queryOptions: {
      refetchInterval: COMMAND_REFETCH_INTERVAL,
    },
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_COMMANDS,
  };
};

export default useCommands;

export const useExecuteCommand = () => {
  const queryClient = useQueryClient();
  const lastCommandRef = useRef<{
    command: NewCommandBody;
    timestamp: number;
  } | null>(null);

  const { mutate } = useApiMutation<Command, NewCommandBody>({
    method: 'POST',
    path: '/command',
    mutationOptions: {
      onSuccess: (newCommand: Command) => {
        queryClient.setQueryData<Command[]>(
          ['/command'],
          (oldCommands = []) => {
            return [...oldCommands, newCommand];
          }
        );
      },
    },
  });

  const executeCommand = useCallback(
    (body: NewCommandBody, commandFinished?: (command: Command) => void) => {
      const now = Date.now();
      const lastCommand = lastCommandRef.current;

      // Check if the same command was run within the last 5 seconds
      if (
        lastCommand &&
        now - lastCommand.timestamp < 5000 &&
        isSameCommand(lastCommand.command, body)
      ) {
        console.warn(
          'Please wait at least 5 seconds before running this command again'
        );
        return;
      }

      // Update last command reference
      lastCommandRef.current = {
        command: body,
        timestamp: now,
      };

      const executeWithCallback = (commandBody: NewCommandBody) => {
        mutate(commandBody, {
          onSuccess: (command) => {
            if (commandFinished) {
              commandFinishedCallbacks[command.id] = commandFinished;
            }
          },
        });
      };

      executeWithCallback(body);
    },
    [mutate]
  );

  return executeCommand;
};

export const useCancelCommand = (id: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<void, void>({
    method: 'DELETE',
    path: `/command/${id}`,
    mutationOptions: {
      onSuccess: () => {
        queryClient.setQueryData<Command[]>(
          ['/command'],
          (oldCommands = []) => {
            return oldCommands.filter((command) => command.id !== id);
          }
        );
      },
    },
  });

  return {
    cancelCommand: mutate,
    isCancellingCommand: isPending,
    commandCancelError: error,
  };
};

export const useCommand = (
  commandName: string,
  constraints: Partial<CommandBody> = {}
) => {
  const { data: commands } = useCommands();

  return useMemo(() => {
    return commands.findLast((command) => {
      if (command.name !== commandName) {
        return false;
      }

      return (Object.keys(constraints) as Array<keyof CommandBody>).every(
        (key) => {
          const constraintValue = constraints[key];
          const commandValue = command.body?.[key];

          if (constraintValue === undefined) {
            return true;
          }

          if (Array.isArray(constraintValue) && Array.isArray(commandValue)) {
            return constraintValue.every((value) =>
              commandValue.includes(value)
            );
          }

          return constraintValue === commandValue;
        }
      );
    });
  }, [commands, commandName, constraints]);
};

export const useCommandExecuting = (
  commandName: string,
  constraints: Partial<CommandBody> = {}
) => {
  const command = useCommand(commandName, constraints);

  return command
    ? command.status === 'queued' || command.status === 'started'
    : false;
};

export const useExecutingCommands = () => {
  const { data: commands } = useCommands();

  return commands.filter(
    (command) => command.status === 'queued' || command.status === 'started'
  );
};

export const useUpdateCommand = () => {
  const queryClient = useQueryClient();

  return (command: Command) => {
    queryClient.setQueryData<Command[]>(['/command'], (oldCommands = []) => {
      return oldCommands.map((existingCommand) =>
        existingCommand.id === command.id ? command : existingCommand
      );
    });

    // Show command message for user feedback
    showCommandMessage(command);

    // Both successful and failed commands need to be
    // completed, otherwise they spin until they time out.
    const isFinished =
      command.status === 'completed' || command.status === 'failed';

    if (isFinished) {
      const commandFinished = commandFinishedCallbacks[command.id];

      if (commandFinished) {
        commandFinished(command);
        delete commandFinishedCallbacks[command.id];
      }
    }
  };
};

function showCommandMessage(command: Command) {
  const {
    id,
    name,
    trigger,
    message,
    body = {} as CommandBody,
    status,
  } = command;

  const { sendUpdatesToClient, suppressMessages } = body;

  if (!message || !body || !sendUpdatesToClient || suppressMessages) {
    return;
  }

  let type: MessageType = INFO;
  let hideAfter = 0;

  if (status === 'completed') {
    type = SUCCESS;
    hideAfter = 4;
  } else if (status === 'failed') {
    type = ERROR;
    hideAfter = trigger === 'manual' ? 10 : 4;
  }

  showMessage({
    id,
    name,
    message,
    type,
    hideAfter,
  });
}
