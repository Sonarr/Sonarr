import ModelBase from 'App/ModelBase';

export type CommandStatus =
  | 'queued'
  | 'started'
  | 'completed'
  | 'failed'
  | 'aborted'
  | 'cancelled'
  | 'orphaned';

export type CommandResult = 'unknown' | 'successful' | 'unsuccessful';

export interface CommandBody {
  sendUpdatesToClient: boolean;
  updateScheduledTask: boolean;
  completionMessage: string;
  requiresDiskAccess: boolean;
  isExclusive: boolean;
  isLongRunning: boolean;
  name: string;
  lastExecutionTime: string;
  lastStartTime: string;
  trigger: string;
  suppressMessages: boolean;
  seriesId?: number;
  seriesIds?: number[];
  seasonNumber?: number;
  episodeIds?: number[];
  [key: string]: string | number | boolean | number[] | undefined;
}

interface Command extends ModelBase {
  name: string;
  commandName: string;
  message: string;
  body: CommandBody;
  priority: string;
  status: CommandStatus;
  result: CommandResult;
  queued: string;
  started: string;
  ended: string;
  duration: string;
  trigger: string;
  stateChangeTime: string;
  sendUpdatesToClient: boolean;
  updateScheduledTask: boolean;
  lastExecutionTime: string;
}

export default Command;
