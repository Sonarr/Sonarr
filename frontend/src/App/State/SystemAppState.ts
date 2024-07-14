import SystemStatus from 'typings/SystemStatus';
import Update from 'typings/Update';
import AppSectionState, { AppSectionItemState } from './AppSectionState';

export type SystemStatusAppState = AppSectionItemState<SystemStatus>;
export type UpdateAppState = AppSectionState<Update>;

interface SystemAppState {
  updates: UpdateAppState;
  status: SystemStatusAppState;
}

export default SystemAppState;
