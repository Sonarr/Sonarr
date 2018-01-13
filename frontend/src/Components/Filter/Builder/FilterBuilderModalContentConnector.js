import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterBuilderModalContent from './FilterBuilderModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { customFilters }) => customFilters,
    (state, { customFilterKey }) => customFilterKey,
    (customFilters, customFilterKey) => {
      if (customFilterKey) {
        const customFilter = customFilters.find((c) => c.key === customFilterKey);

        return {
          customFilterKey: customFilter.key,
          label: customFilter.label,
          filters: customFilter.filters
        };
      }

      return {
        label: '',
        filters: []
      };
    }
  );
}

export default connect(createMapStateToProps)(FilterBuilderModalContent);
