import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import CustomFilter from './CustomFilter';
import styles from './CustomFiltersModalContent.css';

function CustomFiltersModalContent(props) {
  const {
    selectedFilterKey,
    customFilters,
    isDeleting,
    deleteError,
    dispatchDeleteCustomFilter,
    dispatchSetFilter,
    onAddCustomFilter,
    onEditCustomFilter,
    onModalClose
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
          Custom Filters
      </ModalHeader>

      <ModalBody>
        {
          customFilters.map((customFilter) => {
            return (
              <CustomFilter
                key={customFilter.id}
                id={customFilter.id}
                label={customFilter.label}
                filters={customFilter.filters}
                selectedFilterKey={selectedFilterKey}
                isDeleting={isDeleting}
                deleteError={deleteError}
                dispatchSetFilter={dispatchSetFilter}
                dispatchDeleteCustomFilter={dispatchDeleteCustomFilter}
                onEditPress={onEditCustomFilter}
              />
            );
          })
        }

        <div className={styles.addButtonContainer}>
          <Button onPress={onAddCustomFilter}>
            Add Custom Filter
          </Button>
        </div>
      </ModalBody>

      <ModalFooter>
        <Button
          onPress={onModalClose}
        >
            Close
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

CustomFiltersModalContent.propTypes = {
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  dispatchDeleteCustomFilter: PropTypes.func.isRequired,
  dispatchSetFilter: PropTypes.func.isRequired,
  onAddCustomFilter: PropTypes.func.isRequired,
  onEditCustomFilter: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default CustomFiltersModalContent;
