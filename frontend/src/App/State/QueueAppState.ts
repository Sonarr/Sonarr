import Queue from 'typings/Queue';
import AppSectionState, {
  AppSectionFilterState,
  AppSectionItemState,
  Error,
  PagedAppSectionState,
  TableAppSectionState,
} from './AppSectionState';

export interface QueueStatus {
  totalCount: number;
  count: number;
  unknownCount: number;
  errors: boolean;
  warnings: boolean;
  unknownErrors: boolean;
  unknownWarnings: boolean;
}

export interface QueueDetailsAppState extends AppSectionState<Queue> {
  params: unknown;
}

export interface QueuePagedAppState
  extends AppSectionState<Queue>,
    AppSectionFilterState<Queue>,
    PagedAppSectionState,
    TableAppSectionState {
  isGrabbing: boolean;
  grabError: Error;
  isRemoving: boolean;
  removeError: Error;
}

interface QueueAppState {
  status: AppSectionItemState<QueueStatus>;
  details: QueueDetailsAppState;
  paged: QueuePagedAppState;
  options: {
    includeUnknownSeriesItems: boolean;
  };
}

export default QueueAppState;
