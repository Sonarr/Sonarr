import DiskSpace from 'typings/DiskSpace';
import Health from 'typings/Health';
import LogFile from 'typings/LogFile';
import Task from 'typings/Task';
import AppSectionState from './AppSectionState';
import BackupAppState from './BackupAppState';

export type DiskSpaceAppState = AppSectionState<DiskSpace>;
export type HealthAppState = AppSectionState<Health>;
export type TaskAppState = AppSectionState<Task>;
export type LogFilesAppState = AppSectionState<LogFile>;

interface SystemAppState {
  backups: BackupAppState;
  diskSpace: DiskSpaceAppState;
  health: HealthAppState;
  logFiles: LogFilesAppState;
  tasks: TaskAppState;
  updateLogFiles: LogFilesAppState;
}

export default SystemAppState;
