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
    customFilters,
    onAddCustomFilter,
    onRemoveCustomFilterPress,
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
          customFilters.map((customFilter, index) => {
            return (
              <CustomFilter
                key={index}
                customFilterKey={customFilter.key}
                label={customFilter.label}
                filters={customFilter.filters}
                onRemovePress={onRemoveCustomFilterPress}
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
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  onAddCustomFilter: PropTypes.func.isRequired,
  onRemoveCustomFilterPress: PropTypes.func.isRequired,
  onEditCustomFilter: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default CustomFiltersModalContent;
