import SystemStatus from 'typings/SystemStatus';
import { AppSectionItemState } from './AppSectionState';

export type SystemStatusAppState = AppSectionItemState<SystemStatus>;

interface SystemAppState {
  status: SystemStatusAppState;
}

export default SystemAppState;
