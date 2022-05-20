import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteCustomFilter, saveCustomFilter } from 'Store/Actions/customFilterActions';
import FilterBuilderModalContent from './FilterBuilderModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { customFilters }) => customFilters,
    (state, { id }) => id,
    (state) => state.customFilters.isSaving,
    (state) => state.customFilters.saveError,
    (customFilters, id, isSaving, saveError) => {
      if (id) {
        const customFilter = customFilters.find((c) => c.id === id);

        return {
          id: customFilter.id,
          label: customFilter.label,
          filters: customFilter.filters,
          customFilters,
          isSaving,
          saveError
        };
      }

      return {
        label: '',
        filters: [],
        customFilters,
        isSaving,
        saveError
      };
    }
  );
}

const mapDispatchToProps = {
  onSaveCustomFilterPress: saveCustomFilter,
  dispatchDeleteCustomFilter: deleteCustomFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterBuilderModalContent);
