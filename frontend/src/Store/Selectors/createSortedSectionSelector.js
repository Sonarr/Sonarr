import { createSelector } from 'reselect';
import getSectionState from 'Utilities/State/getSectionState';

function createSortedSectionSelector(section, comparer) {
  return createSelector(
    (state) => state,
    (state) => {
      const sectionState = getSectionState(state, section, true);
      return {
        ...sectionState,
        items: [...sectionState.items].sort(comparer)
      };
    }
  );
}

export default createSortedSectionSelector;
