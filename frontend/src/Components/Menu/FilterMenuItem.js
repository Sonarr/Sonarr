import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SelectedMenuItem from './SelectedMenuItem';

class FilterMenuItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      filterKey,
      onPress
    } = this.props;

    onPress(filterKey);
  };

  //
  // Render

  render() {
    const {
      filterKey,
      selectedFilterKey,
      ...otherProps
    } = this.props;

    return (
      <SelectedMenuItem
        isSelected={filterKey === selectedFilterKey}
        {...otherProps}
        onPress={this.onPress}
      />
    );
  }
}

FilterMenuItem.propTypes = {
  filterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  onPress: PropTypes.func.isRequired
};

export default FilterMenuItem;
