import ModelBase from 'App/ModelBase';

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
}

interface Command extends ModelBase {
  name: string;
  commandName: string;
  message: string;
  body: CommandBody;
  priority: string;
  status: string;
  result: string;
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
