import AppSectionState, {
  AppSectionFilterState,
  PagedAppSectionState,
  TableAppSectionState,
} from 'App/State/AppSectionState';
import Episode from 'Episode/Episode';

interface WantedEpisode extends Episode {
  isSaving?: boolean;
}

interface WantedCutoffUnmetAppState
  extends AppSectionState<WantedEpisode>,
    AppSectionFilterState<WantedEpisode>,
    PagedAppSectionState,
    TableAppSectionState {}

interface WantedMissingAppState
  extends AppSectionState<WantedEpisode>,
    AppSectionFilterState<WantedEpisode>,
    PagedAppSectionState,
    TableAppSectionState {}

interface WantedAppState {
  cutoffUnmet: WantedCutoffUnmetAppState;
  missing: WantedMissingAppState;
}

export default WantedAppState;
