import AppSectionState, {
  AppSectionFilterState,
} from 'App/State/AppSectionState';
import History from 'typings/History';

interface HistoryAppState
  extends AppSectionState<History>,
    AppSectionFilterState<History> {}

export default HistoryAppState;
