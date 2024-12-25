import AppSectionState, {
  AppSectionFilterState,
  PagedAppSectionState,
  TableAppSectionState,
} from 'App/State/AppSectionState';
import LogEvent from 'typings/LogEvent';

interface LogsAppState
  extends AppSectionState<LogEvent>,
    AppSectionFilterState<LogEvent>,
    PagedAppSectionState,
    TableAppSectionState {}

export default LogsAppState;
