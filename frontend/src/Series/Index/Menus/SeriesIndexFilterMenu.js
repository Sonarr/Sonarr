import PropTypes from 'prop-types';
import React from 'react';
import { align } from 'Helpers/Props';
import FilterMenu from 'Components/Menu/FilterMenu';
import MenuContent from 'Components/Menu/MenuContent';
import FilterMenuItem from 'Components/Menu/FilterMenuItem';

function SeriesIndexFilterMenu(props) {
  const {
    filterKey,
    filterValue,
    onFilterSelect
  } = props;

  return (
    <FilterMenu alignMenu={align.RIGHT}>
      <MenuContent>
        <FilterMenuItem
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          All
        </FilterMenuItem>

        <FilterMenuItem
          name="monitored"
          value={true}
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          Monitored Only
        </FilterMenuItem>

        <FilterMenuItem
          name="status"
          value="continuing"
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          Continuing Only
        </FilterMenuItem>

        <FilterMenuItem
          name="status"
          value="ended"
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          Ended Only
        </FilterMenuItem>

        <FilterMenuItem
          name="missing"
          value={true}
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          Missing Episodes
        </FilterMenuItem>
      </MenuContent>
    </FilterMenu>
  );
}

SeriesIndexFilterMenu.propTypes = {
  filterKey: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.bool, PropTypes.number, PropTypes.string]),
  onFilterSelect: PropTypes.func.isRequired
};

export default SeriesIndexFilterMenu;
