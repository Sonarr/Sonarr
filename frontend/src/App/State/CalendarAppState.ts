import AppSectionState from 'App/State/AppSectionState';
import Episode from 'Episode/Episode';
import { FilterBuilderProp } from './AppState';

interface CalendarAppState extends AppSectionState<Episode> {
  filterBuilderProps: FilterBuilderProp<Episode>[];
}

export default CalendarAppState;
