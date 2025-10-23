import LogFile from 'typings/LogFile';
import Task from 'typings/Task';
import AppSectionState from './AppSectionState';
import BackupAppState from './BackupAppState';

export type TaskAppState = AppSectionState<Task>;
export type LogFilesAppState = AppSectionState<LogFile>;

interface SystemAppState {
  backups: BackupAppState;
  logFiles: LogFilesAppState;
  tasks: TaskAppState;
  updateLogFiles: LogFilesAppState;
}

export default SystemAppState;
