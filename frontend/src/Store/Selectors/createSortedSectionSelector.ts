import { createSelector } from 'reselect';
import AppSectionState, {
  AppSectionProviderState,
} from 'App/State/AppSectionState';
import AppState from 'App/State/AppState';
import getSectionState from 'Utilities/State/getSectionState';

function createSortedSectionSelector<
  T,
  S extends AppSectionState<T> | AppSectionProviderState<T>
>(section: string, comparer: (a: T, b: T) => number) {
  return createSelector(
    (state: AppState) => state,
    (state) => {
      const sectionState = getSectionState(state, section, true) as S;

      return {
        ...sectionState,
        items: [...sectionState.items].sort(comparer),
      };
    }
  );
}

export default createSortedSectionSelector;
