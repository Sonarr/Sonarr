import AppSectionState, {
  AppSectionFilterState,
  PagedAppSectionState,
  TableAppSectionState,
} from 'App/State/AppSectionState';
import History from 'typings/History';

export type SeriesHistoryAppState = AppSectionState<History>;

interface HistoryAppState
  extends AppSectionState<History>,
    AppSectionFilterState<History>,
    PagedAppSectionState,
    TableAppSectionState {}

export default HistoryAppState;
