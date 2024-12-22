import React, { useCallback, useEffect, useState } from 'react';
import { FilterBuilderProp, PropertyFilter } from 'App/State/AppState';
import SelectInput from 'Components/Form/SelectInput';
import IconButton from 'Components/Link/IconButton';
import { filterBuilderValueTypes, icons } from 'Helpers/Props';
import {
  FilterBuilderTypes,
  possibleFilterTypes,
} from 'Helpers/Props/filterBuilderTypes';
import { InputChanged } from 'typings/inputs';
import sortByProp from 'Utilities/Array/sortByProp';
import BoolFilterBuilderRowValue from './BoolFilterBuilderRowValue';
import DateFilterBuilderRowValue, {
  DateFilterValue,
} from './DateFilterBuilderRowValue';
import DefaultFilterBuilderRowValue from './DefaultFilterBuilderRowValue';
import HistoryEventTypeFilterBuilderRowValue from './HistoryEventTypeFilterBuilderRowValue';
import IndexerFilterBuilderRowValue from './IndexerFilterBuilderRowValue';
import LanguageFilterBuilderRowValue from './LanguageFilterBuilderRowValue';
import ProtocolFilterBuilderRowValue from './ProtocolFilterBuilderRowValue';
import QualityFilterBuilderRowValue from './QualityFilterBuilderRowValue';
import QualityProfileFilterBuilderRowValue from './QualityProfileFilterBuilderRowValue';
import QueueStatusFilterBuilderRowValue from './QueueStatusFilterBuilderRowValue';
import SeasonsMonitoredStatusFilterBuilderRowValue from './SeasonsMonitoredStatusFilterBuilderRowValue';
import SeriesFilterBuilderRowValue from './SeriesFilterBuilderRowValue';
import SeriesStatusFilterBuilderRowValue from './SeriesStatusFilterBuilderRowValue';
import SeriesTypeFilterBuilderRowValue from './SeriesTypeFilterBuilderRowValue';
import TagFilterBuilderRowValue from './TagFilterBuilderRowValue';
import styles from './FilterBuilderRow.css';

function getselectedFilterBuilderProp<T>(
  filterBuilderProps: FilterBuilderProp<T>[],
  name: string
) {
  return filterBuilderProps.find((a) => {
    return a.name === name;
  });
}

function getFilterTypeOptions<T>(
  filterBuilderProps: FilterBuilderProp<T>[],
  filterKey: string | undefined
) {
  if (!filterKey) {
    return [];
  }

  const selectedFilterBuilderProp = getselectedFilterBuilderProp(
    filterBuilderProps,
    filterKey
  );

  if (!selectedFilterBuilderProp) {
    return [];
  }

  return possibleFilterTypes[selectedFilterBuilderProp.type];
}

function getDefaultFilterType<T>(
  selectedFilterBuilderProp: FilterBuilderProp<T>
) {
  return possibleFilterTypes[selectedFilterBuilderProp.type][0].key;
}

function getDefaultFilterValue<T>(
  selectedFilterBuilderProp: FilterBuilderProp<T>
) {
  if (selectedFilterBuilderProp.type === 'date') {
    return '';
  }

  return [];
}

function getRowValueConnector<T>(
  selectedFilterBuilderProp: FilterBuilderProp<T> | undefined
) {
  if (!selectedFilterBuilderProp) {
    return DefaultFilterBuilderRowValue;
  }

  const valueType = selectedFilterBuilderProp.valueType;

  switch (valueType) {
    case filterBuilderValueTypes.BOOL:
      return BoolFilterBuilderRowValue;

    case filterBuilderValueTypes.DATE:
      return DateFilterBuilderRowValue;

    case filterBuilderValueTypes.HISTORY_EVENT_TYPE:
      return HistoryEventTypeFilterBuilderRowValue;

    case filterBuilderValueTypes.INDEXER:
      return IndexerFilterBuilderRowValue;

    case filterBuilderValueTypes.LANGUAGE:
      return LanguageFilterBuilderRowValue;

    case filterBuilderValueTypes.PROTOCOL:
      return ProtocolFilterBuilderRowValue;

    case filterBuilderValueTypes.QUALITY:
      return QualityFilterBuilderRowValue;

    case filterBuilderValueTypes.QUALITY_PROFILE:
      return QualityProfileFilterBuilderRowValue;

    case filterBuilderValueTypes.QUEUE_STATUS:
      return QueueStatusFilterBuilderRowValue;

    case filterBuilderValueTypes.SEASONS_MONITORED_STATUS:
      return SeasonsMonitoredStatusFilterBuilderRowValue;

    case filterBuilderValueTypes.SERIES:
      return SeriesFilterBuilderRowValue;

    case filterBuilderValueTypes.SERIES_STATUS:
      return SeriesStatusFilterBuilderRowValue;

    case filterBuilderValueTypes.SERIES_TYPES:
      return SeriesTypeFilterBuilderRowValue;

    case filterBuilderValueTypes.TAG:
      return TagFilterBuilderRowValue;

    default:
      return DefaultFilterBuilderRowValue;
  }
}

interface FilterBuilderRowProps<T> {
  index: number;
  filterKey?: string;
  filterValue?: (DateFilterValue | string) | string[] | number[] | boolean[];
  filterType?: string;
  filterCount: number;
  filterBuilderProps: FilterBuilderProp<T>[];
  sectionItems: T[];
  onAddPress: () => void;
  onFilterChange: (index: number, filter: PropertyFilter) => void;
  onRemovePress: (index: number) => void;
}

function FilterBuilderRow<T>({
  index,
  filterKey,
  filterType,
  filterValue,
  filterCount,
  filterBuilderProps,
  sectionItems,
  onAddPress,
  onFilterChange,
  onRemovePress,
}: FilterBuilderRowProps<T>) {
  const [selectedFilterBuilderProp, setSelectedFilterBuilderProp] = useState<
    FilterBuilderProp<T> | undefined
  >(undefined);

  const keyOptions = filterBuilderProps
    .map((availablePropFilter) => {
      const { name, label } = availablePropFilter;

      return {
        key: name,
        value: typeof label === 'function' ? label() : label,
      };
    })
    .sort(sortByProp('value'));

  const ValueComponent = getRowValueConnector(selectedFilterBuilderProp);

  const handleFilterKeyChange = useCallback(
    ({ value: key }: InputChanged<FilterBuilderTypes>) => {
      const selected = getselectedFilterBuilderProp(filterBuilderProps, key);

      if (!selected) {
        return;
      }

      const type = getDefaultFilterType(selected);

      const filter = {
        key,
        value: getDefaultFilterValue(selected),
        type,
      };

      setSelectedFilterBuilderProp(selected);
      onFilterChange(index, filter);
    },
    [filterBuilderProps, index, onFilterChange]
  );

  const handleFilterTypeChange = useCallback(
    ({ value }: InputChanged<string>) => {
      if (filterKey == null || filterValue == null || filterType == null) {
        return;
      }

      const filter: PropertyFilter = {
        key: filterKey,
        value: filterValue,
        type: value,
      };

      onFilterChange(index, filter);
    },
    [index, filterKey, filterValue, filterType, onFilterChange]
  );

  const handleFilterValueChange = useCallback(
    ({
      value,
    }: InputChanged<
      string | boolean[] | string[] | number[] | DateFilterValue
    >) => {
      if (filterKey == null || filterValue == null || filterType == null) {
        return;
      }

      const filter: PropertyFilter = {
        key: filterKey,
        value,
        type: filterType,
      };

      onFilterChange(index, filter);
    },
    [index, filterKey, filterValue, filterType, onFilterChange]
  );

  const handleAddPress = useCallback(() => {
    onAddPress();
  }, [onAddPress]);

  const handleRemovePress = useCallback(() => {
    onRemovePress(index);
  }, [index, onRemovePress]);

  useEffect(() => {
    if (filterKey) {
      setSelectedFilterBuilderProp(
        filterBuilderProps.find((a) => a.name === filterKey)
      );

      return;
    }

    const selected = filterBuilderProps[0];

    const filter = {
      key: selected.name,
      value: getDefaultFilterValue(selected),
      type: getDefaultFilterType(selected),
    };

    setSelectedFilterBuilderProp(selected);
    onFilterChange(index, filter);
  }, [index, filterKey, filterBuilderProps, onFilterChange]);

  return (
    <div className={styles.filterRow}>
      <div className={styles.inputContainer}>
        {filterKey ? (
          <SelectInput
            name="key"
            value={filterKey}
            values={keyOptions}
            onChange={handleFilterKeyChange}
          />
        ) : null}
      </div>

      <div className={styles.inputContainer}>
        {filterType ? (
          <SelectInput
            name="type"
            value={filterType}
            values={getFilterTypeOptions(filterBuilderProps, filterKey)}
            onChange={handleFilterTypeChange}
          />
        ) : null}
      </div>

      <div className={styles.valueInputContainer}>
        {filterValue != null && !!selectedFilterBuilderProp ? (
          <ValueComponent
            filterType={filterType}
            // @ts-expect-error - inferring the correct value type is hard
            filterValue={filterValue}
            selectedFilterBuilderProp={selectedFilterBuilderProp}
            sectionItems={sectionItems}
            onChange={handleFilterValueChange}
          />
        ) : null}
      </div>

      <div className={styles.actionsContainer}>
        <IconButton
          name={icons.SUBTRACT}
          isDisabled={filterCount === 1}
          onPress={handleRemovePress}
        />

        <IconButton name={icons.ADD} onPress={handleAddPress} />
      </div>
    </div>
  );
}

export default FilterBuilderRow;
