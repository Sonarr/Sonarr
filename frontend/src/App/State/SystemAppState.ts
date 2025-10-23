import Task from 'typings/Task';
import AppSectionState from './AppSectionState';
import BackupAppState from './BackupAppState';

export type TaskAppState = AppSectionState<Task>;

interface SystemAppState {
  backups: BackupAppState;
  tasks: TaskAppState;
}

export default SystemAppState;
