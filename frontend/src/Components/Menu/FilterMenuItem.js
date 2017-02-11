import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SelectedMenuItem from './SelectedMenuItem';

class FilterMenuItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      name,
      value,
      onPress
    } = this.props;

    onPress(name, value);
  }

  //
  // Render

  render() {
    const {
      name,
      value,
      filterKey,
      filterValue,
      ...otherProps
    } = this.props;

    const isSelected = name === filterKey && value === filterValue;

    return (
      <SelectedMenuItem
        isSelected={isSelected}
        {...otherProps}
        onPress={this.onPress}
      />
    );
  }
}

FilterMenuItem.propTypes = {
  name: PropTypes.string,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.bool]),
  filterKey: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.bool]),
  onPress: PropTypes.func.isRequired
};

FilterMenuItem.defaultProps = {
  name: null,
  value: null
};

export default FilterMenuItem;
