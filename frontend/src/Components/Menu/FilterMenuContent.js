import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MenuContent from './MenuContent';
import FilterMenuItem from './FilterMenuItem';
import MenuItem from './MenuItem';
import MenuItemSeparator from './MenuItemSeparator';

class FilterMenuContent extends Component {

  //
  // Render

  render() {
    const {
      selectedFilterKey,
      filters,
      customFilters,
      showCustomFilters,
      onFilterSelect,
      onCustomFiltersPress,
      ...otherProps
    } = this.props;

    return (
      <MenuContent {...otherProps}>
        {
          filters.map((filter) => {
            return (
              <FilterMenuItem
                key={filter.key}
                filterKey={filter.key}
                selectedFilterKey={selectedFilterKey}
                onPress={onFilterSelect}
              >
                {filter.label}
              </FilterMenuItem>
            );
          })
        }

        {
          customFilters.map((filter) => {
            return (
              <FilterMenuItem
                key={filter.key}
                filterKey={filter.key}
                selectedFilterKey={selectedFilterKey}
                onPress={onFilterSelect}
              >
                {filter.label}
              </FilterMenuItem>
            );
          })
        }

        {
          showCustomFilters &&
            <MenuItemSeparator />
        }

        {
          showCustomFilters &&
            <MenuItem onPress={onCustomFiltersPress}>
                Custom Filters
            </MenuItem>
        }
      </MenuContent>
    );
  }
}

FilterMenuContent.propTypes = {
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  showCustomFilters: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onCustomFiltersPress: PropTypes.func.isRequired
};

FilterMenuContent.defaultProps = {
  showCustomFilters: false
};

export default FilterMenuContent;
