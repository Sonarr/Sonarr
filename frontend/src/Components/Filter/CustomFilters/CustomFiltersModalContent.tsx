import React from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { CustomFilter as CustomFilterModel } from 'Filters/Filter';
import translate from 'Utilities/String/translate';
import CustomFilter from './CustomFilter';
import styles from './CustomFiltersModalContent.css';

interface CustomFiltersModalContentProps {
  customFilters: CustomFilterModel[];
  dispatchSetFilter: (payload: { selectedFilterKey: string | number }) => void;
  onAddCustomFilter: () => void;
  onEditCustomFilter: (id: number) => void;
  onModalClose: () => void;
}

function CustomFiltersModalContent({
  customFilters,
  dispatchSetFilter,
  onAddCustomFilter,
  onEditCustomFilter,
  onModalClose,
}: CustomFiltersModalContentProps) {
  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('CustomFilters')}</ModalHeader>

      <ModalBody>
        {customFilters.map((customFilter) => {
          return (
            <CustomFilter
              key={customFilter.id}
              id={customFilter.id}
              label={customFilter.label}
              dispatchSetFilter={dispatchSetFilter}
              onEditPress={onEditCustomFilter}
            />
          );
        })}

        <div className={styles.addButtonContainer}>
          <Button onPress={onAddCustomFilter}>
            {translate('AddCustomFilter')}
          </Button>
        </div>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default CustomFiltersModalContent;
