import React from 'react';
import { useSelector } from 'react-redux';
import AppState, {
  CustomFilter as CustomFilterModel,
} from 'App/State/AppState';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import sortByProp from 'Utilities/Array/sortByProp';
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
  const { isDeleting, deleteError } = useSelector(
    (state: AppState) => state.customFilters
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('CustomFilters')}</ModalHeader>

      <ModalBody>
        {customFilters.sort(sortByProp('label')).map((customFilter) => {
          return (
            <CustomFilter
              key={customFilter.id}
              id={customFilter.id}
              label={customFilter.label}
              isDeleting={isDeleting}
              deleteError={deleteError}
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
