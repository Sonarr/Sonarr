import AppSectionState, {
  AppSectionFilterState,
} from 'App/State/AppSectionState';
import Release from 'typings/Release';

interface ReleasesAppState
  extends AppSectionState<Release>,
    AppSectionFilterState<Release> {}

export default ReleasesAppState;
