import { maxBy } from 'lodash';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import FormInputGroup, {
  ValidationMessage,
} from 'Components/Form/FormInputGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import {
  CustomFilter,
  FilterBuilderProp,
  PropertyFilter,
} from 'Filters/Filter';
import { useSaveCustomFilter } from 'Filters/useCustomFilters';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import FilterBuilderRow from './FilterBuilderRow';
import styles from './FilterBuilderModalContent.css';

const NEW_FILTER: PropertyFilter = {
  key: '',
  value: '',
  type: 'unknown',
};

interface FilterBuilderModalContentProps<T> {
  id: number | null;
  customFilterType: string;
  sectionItems: T[];
  filterBuilderProps: FilterBuilderProp<T>[];
  customFilters: CustomFilter[];
  dispatchSetFilter: (payload: { selectedFilterKey: string | number }) => void;
  onCancelPress: () => void;
  onModalClose: () => void;
}

function FilterBuilderModalContent<T>({
  id,
  customFilters,
  customFilterType,
  sectionItems,
  filterBuilderProps,
  dispatchSetFilter,
  onCancelPress,
  onModalClose,
}: FilterBuilderModalContentProps<T>) {
  const { newCustomFilter, saveCustomFilter, isSaving, saveError } =
    useSaveCustomFilter(id);

  const { initialLabel, initialFilters } = useMemo(() => {
    if (id) {
      const customFilter = customFilters.find((c) => c.id === id);

      if (customFilter) {
        return {
          initialLabel: customFilter.label,
          initialFilters: customFilter.filters,
        };
      }
    }

    return {
      initialLabel: '',
      initialFilters: [],
    };
  }, [id, customFilters]);

  const [label, setLabel] = useState(initialLabel);
  // Push an empty filter if there aren't any filters. FilterBuilderRow
  // will handle initializing the filter.
  const [filters, setFilters] = useState<PropertyFilter[]>(
    initialFilters.length ? initialFilters : [NEW_FILTER]
  );
  const [labelErrors, setLabelErrors] = useState<ValidationMessage[]>([]);
  const wasSaving = usePrevious(isSaving);

  const handleLabelChange = useCallback(({ value }: InputChanged<string>) => {
    setLabel(value);
  }, []);

  const handleFilterChange = useCallback(
    (index: number, filter: PropertyFilter) => {
      const newFilters = [...filters];
      newFilters.splice(index, 1, filter);

      setFilters(newFilters);
    },
    [filters]
  );

  const handleAddFilterPress = useCallback(() => {
    setFilters([...filters, NEW_FILTER]);
  }, [filters]);

  const handleRemoveFilterPress = useCallback(
    (index: number) => {
      const newFilters = [...filters];
      newFilters.splice(index, 1);

      setFilters(newFilters);
    },
    [filters]
  );

  const handleSaveFilterPress = useCallback(() => {
    if (!label) {
      setLabelErrors([
        {
          message: translate('LabelIsRequired'),
        },
      ]);

      return;
    }

    saveCustomFilter({ type: customFilterType, label, filters });
  }, [customFilterType, label, filters, saveCustomFilter]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      if (id) {
        dispatchSetFilter({ selectedFilterKey: id });
      } else if (newCustomFilter) {
        dispatchSetFilter({ selectedFilterKey: newCustomFilter.id });
      } else {
        const last = maxBy(customFilters, 'id');

        if (last) {
          dispatchSetFilter({ selectedFilterKey: last.id });
        }
      }

      onModalClose();
    }
  }, [
    id,
    customFilters,
    newCustomFilter,
    isSaving,
    wasSaving,
    saveError,
    dispatchSetFilter,
    onModalClose,
  ]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('CustomFilter')}</ModalHeader>

      <ModalBody>
        <div className={styles.labelContainer}>
          <div className={styles.label}>{translate('Label')}</div>

          <div className={styles.labelInputContainer}>
            <FormInputGroup
              name="label"
              value={label}
              type={inputTypes.TEXT}
              errors={labelErrors}
              onChange={handleLabelChange}
            />
          </div>
        </div>

        <div className={styles.label}>{translate('Filters')}</div>

        <div className={styles.rows}>
          {filters.map((filter, index) => {
            return (
              <FilterBuilderRow
                key={`${filter.key}-${index}`}
                index={index}
                sectionItems={sectionItems}
                filterBuilderProps={filterBuilderProps}
                filterKey={filter.key}
                filterValue={filter.value}
                filterType={filter.type}
                filterCount={filters.length}
                onAddPress={handleAddFilterPress}
                onRemovePress={handleRemoveFilterPress}
                onFilterChange={handleFilterChange}
              />
            );
          })}
        </div>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onCancelPress}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSaveFilterPress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default FilterBuilderModalContent;
