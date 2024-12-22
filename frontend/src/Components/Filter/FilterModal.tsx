import React, { useCallback, useState } from 'react';
import { CustomFilter, FilterBuilderProp } from 'App/State/AppState';
import Modal from 'Components/Modal/Modal';
import FilterBuilderModalContent from './Builder/FilterBuilderModalContent';
import CustomFiltersModalContent from './CustomFilters/CustomFiltersModalContent';

export interface FilterModalProps<T> {
  isOpen: boolean;
  customFilters: CustomFilter[];
  customFilterType: string;
  filterBuilderProps: FilterBuilderProp<T>[];
  sectionItems: T[];
  dispatchSetFilter: (payload: { selectedFilterKey: string | number }) => void;
  onModalClose: () => void;
}

function FilterModal<T>({
  isOpen,
  customFilters,
  onModalClose,
  ...otherProps
}: FilterModalProps<T>) {
  const [id, setId] = useState<null | number>(null);
  const [filterBuilder, setFilterBuilder] = useState(!customFilters.length);

  const handleAddCustomFilter = useCallback(() => {
    setFilterBuilder(true);
  }, []);

  const handleEditCustomFilter = useCallback((id: number) => {
    setId(id);
    setFilterBuilder(true);
  }, []);

  const handleCancelPress = useCallback(() => {
    if (filterBuilder) {
      setId(null);
      setFilterBuilder(false);
    } else {
      onModalClose();
    }
  }, [filterBuilder, onModalClose]);

  const handleModalClose = useCallback(() => {
    setId(null);
    setFilterBuilder(false);
    onModalClose();
  }, [onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleModalClose}>
      {filterBuilder ? (
        <FilterBuilderModalContent
          {...otherProps}
          id={id}
          customFilters={customFilters}
          onCancelPress={handleCancelPress}
          onModalClose={handleModalClose}
        />
      ) : (
        <CustomFiltersModalContent
          {...otherProps}
          customFilters={customFilters}
          onAddCustomFilter={handleAddCustomFilter}
          onEditCustomFilter={handleEditCustomFilter}
          onModalClose={handleModalClose}
        />
      )}
    </Modal>
  );
}

export default FilterModal;
