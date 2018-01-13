import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import FilterMenuContent from './FilterMenuContent';
import Menu from './Menu';
import ToolbarMenuButton from './ToolbarMenuButton';
import styles from './FilterMenu.css';

class FilterMenu extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isFilterModalOpen: false
    };
  }

  //
  // Listeners

  onCustomFiltersPress = () => {
    this.setState({ isFilterModalOpen: true });
  }

  onFiltersModalClose = () => {
    this.setState({ isFilterModalOpen: false });
  }

  //
  // Render

  render(props) {
    const {
      className,
      isDisabled,
      selectedFilterKey,
      filters,
      customFilters,
      buttonComponent: ButtonComponent,
      filterModalConnectorComponent: FilterModalConnectorComponent,
      onFilterSelect,
      ...otherProps
    } = this.props;

    const showCustomFilters = !!FilterModalConnectorComponent;

    return (
      <div>
        <Menu
          className={className}
          {...otherProps}
        >
          <ButtonComponent
            iconName={icons.FILTER}
            text="Filter"
            isDisabled={isDisabled}
          />

          <FilterMenuContent
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={customFilters}
            showCustomFilters={showCustomFilters}
            onFilterSelect={onFilterSelect}
            onCustomFiltersPress={this.onCustomFiltersPress}
          />

        </Menu>

        {
          showCustomFilters &&
            <FilterModalConnectorComponent
              isOpen={this.state.isFilterModalOpen}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              onFilterSelect={onFilterSelect}
              onModalClose={this.onFiltersModalClose}
            />
        }
      </div>
    );
  }
}

FilterMenu.propTypes = {
  className: PropTypes.string,
  isDisabled: PropTypes.bool.isRequired,
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  buttonComponent: PropTypes.func.isRequired,
  filterModalConnectorComponent: PropTypes.func,
  onFilterSelect: PropTypes.func.isRequired
};

FilterMenu.defaultProps = {
  className: styles.filterMenu,
  isDisabled: false,
  buttonComponent: ToolbarMenuButton
};

export default FilterMenu;
