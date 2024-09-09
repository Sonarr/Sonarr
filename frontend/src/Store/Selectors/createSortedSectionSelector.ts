import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import getSectionState from 'Utilities/State/getSectionState';

function createSortedSectionSelector<T>(
  section: string,
  comparer: (a: T, b: T) => number
) {
  return createSelector(
    (state: AppState) => state,
    (state) => {
      const sectionState = getSectionState(state, section, true);

      return {
        ...sectionState,
        items: [...sectionState.items].sort(comparer),
      };
    }
  );
}

export default createSortedSectionSelector;
