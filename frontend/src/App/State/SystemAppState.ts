import DiskSpace from 'typings/DiskSpace';
import Health from 'typings/Health';
import SystemStatus from 'typings/SystemStatus';
import Task from 'typings/Task';
import Update from 'typings/Update';
import AppSectionState, { AppSectionItemState } from './AppSectionState';
import BackupAppState from './BackupAppState';
import LogsAppState from './LogsAppState';

export type DiskSpaceAppState = AppSectionState<DiskSpace>;
export type HealthAppState = AppSectionState<Health>;
export type SystemStatusAppState = AppSectionItemState<SystemStatus>;
export type TaskAppState = AppSectionState<Task>;
export type UpdateAppState = AppSectionState<Update>;

interface SystemAppState {
  backups: BackupAppState;
  diskSpace: DiskSpaceAppState;
  health: HealthAppState;
  logs: LogsAppState;
  status: SystemStatusAppState;
  tasks: TaskAppState;
  updates: UpdateAppState;
}

export default SystemAppState;
