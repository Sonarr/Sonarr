import AppSectionState, {
  AppSectionDeleteState,
} from 'App/State/AppSectionState';
import { EpisodeFile } from 'EpisodeFile/EpisodeFile';

interface EpisodeFilesAppState
  extends AppSectionState<EpisodeFile>,
    AppSectionDeleteState {}

export default EpisodeFilesAppState;
