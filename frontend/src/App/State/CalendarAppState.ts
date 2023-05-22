import AppSectionState, {
  AppSectionFilterState,
} from 'App/State/AppSectionState';
import Episode from 'Episode/Episode';

interface CalendarAppState
  extends AppSectionState<Episode>,
    AppSectionFilterState<Episode> {}

export default CalendarAppState;
