import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteCustomFilter } from 'Store/Actions/customFilterActions';
import CustomFiltersModalContent from './CustomFiltersModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.customFilters.isDeleting,
    (state) => state.customFilters.deleteError,
    (isDeleting, deleteError) => {
      return {
        isDeleting,
        deleteError
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchDeleteCustomFilter: deleteCustomFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(CustomFiltersModalContent);
