import PropTypes from 'prop-types';
import React from 'react';
import { align } from 'Helpers/Props';
import FilterMenu from 'Components/Menu/FilterMenu';

function SeriesIndexFilterMenu(props) {
  const {
    selectedFilterKey,
    filters,
    customFilters,
    isDisabled,
    onFilterSelect
  } = props;

  return (
    <FilterMenu
      alignMenu={align.RIGHT}
      isDisabled={isDisabled}
      selectedFilterKey={selectedFilterKey}
      filters={filters}
      customFilters={customFilters}
      onFilterSelect={onFilterSelect}
    />
  );
}

SeriesIndexFilterMenu.propTypes = {
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDisabled: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

SeriesIndexFilterMenu.defaultProps = {
  showCustomFilters: false
};

export default SeriesIndexFilterMenu;
