import AppSectionState, {
  AppSectionDeleteState,
} from 'App/State/AppSectionState';
import { CustomFilter } from './AppState';

interface CustomFiltersAppState
  extends AppSectionState<CustomFilter>,
    AppSectionDeleteState {}

export default CustomFiltersAppState;
