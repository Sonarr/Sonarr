import Queue from 'typings/Queue';
import AppSectionState, {
  AppSectionFilterState,
  AppSectionItemState,
  Error,
} from './AppSectionState';

export interface QueueDetailsAppState extends AppSectionState<Queue> {
  params: unknown;
}

export interface QueuePagedAppState
  extends AppSectionState<Queue>,
    AppSectionFilterState<Queue> {
  isGrabbing: boolean;
  grabError: Error;
  isRemoving: boolean;
  removeError: Error;
}

interface QueueAppState {
  status: AppSectionItemState<Queue>;
  details: QueueDetailsAppState;
  paged: QueuePagedAppState;
}

export default QueueAppState;
