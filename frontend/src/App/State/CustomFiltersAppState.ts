import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import { CustomFilter } from './AppState';

interface CustomFiltersAppState
  extends AppSectionState<CustomFilter>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export default CustomFiltersAppState;
