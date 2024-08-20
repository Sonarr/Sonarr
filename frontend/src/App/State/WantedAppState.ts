import AppSectionState from 'App/State/AppSectionState';
import Episode from 'Episode/Episode';

interface WantedCutoffUnmetAppState extends AppSectionState<Episode> {}

interface WantedMissingAppState extends AppSectionState<Episode> {}

interface WantedAppState {
  cutoffUnmet: WantedCutoffUnmetAppState;
  missing: WantedMissingAppState;
}

export default WantedAppState;
