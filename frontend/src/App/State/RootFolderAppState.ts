import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import RootFolder from 'typings/RootFolder';

interface RootFolderAppState
  extends AppSectionState<RootFolder>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export default RootFolderAppState;
