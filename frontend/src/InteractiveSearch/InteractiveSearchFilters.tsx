import classNames from 'classnames';
import React, { useCallback } from 'react';
import TextInput from 'Components/Form/TextInput';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { align } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import InteractiveSearchFilterModal from './InteractiveSearchFilterModal';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import InteractiveSearchSortMenu from './InteractiveSearchSortMenu';
import {
  getSelectedFilterKeyOption,
  setReleaseOption,
  useReleaseOption,
} from './releaseOptionsStore';
import { FILTERS } from './useReleases';
import styles from './InteractiveSearchFilters.module.css';

interface InteractiveSearchFiltersProps {
  className?: string;
  searchPayload: InteractiveSearchPayload;
  filter: string;
  onFilterChange: (filter: string) => void;
}

function InteractiveSearchFilters({
  className,
  searchPayload,
  filter,
  onFilterChange,
}: InteractiveSearchFiltersProps) {
  const customFilters = useCustomFiltersList('releases');
  const filterKeyOption = getSelectedFilterKeyOption(searchPayload);
  const selectedFilterKey = useReleaseOption(filterKeyOption);

  const handleFilterChange = useCallback(
    ({ value }: InputChanged<string>) => {
      onFilterChange(value);
    },
    [onFilterChange]
  );

  const handleFilterSelect = useCallback(
    (key: string | number) => {
      setReleaseOption(filterKeyOption, key);
    },
    [filterKeyOption]
  );

  return (
    <div className={classNames(styles.filters, className)}>
      <div className={styles.filterInput}>
        <TextInput
          name="releaseFilter"
          value={filter}
          placeholder={translate('FilterReleasesPlaceholder')}
          onChange={handleFilterChange}
        />
      </div>

      <InteractiveSearchSortMenu />

      <FilterMenu
        alignMenu={align.RIGHT}
        selectedFilterKey={selectedFilterKey}
        filters={FILTERS}
        customFilters={customFilters}
        buttonComponent={PageMenuButton}
        filterModalConnectorComponent={InteractiveSearchFilterModal}
        filterModalConnectorComponentProps={{ searchPayload }}
        onFilterSelect={handleFilterSelect}
      />
    </div>
  );
}

export default InteractiveSearchFilters;
