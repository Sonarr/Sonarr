import AppSectionState from 'App/State/AppSectionState';
import Episode from 'Episode/Episode';

type WantedCutoffUnmetAppState = AppSectionState<Episode>;

type WantedMissingAppState = AppSectionState<Episode>;

interface WantedAppState {
  cutoffUnmet: WantedCutoffUnmetAppState;
  missing: WantedMissingAppState;
}

export default WantedAppState;
