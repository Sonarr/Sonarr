import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MenuItem from 'Components/Menu/MenuItem';

class AddImportListPresetMenuItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      name,
      implementation
    } = this.props;

    this.props.onPress({
      name,
      implementation
    });
  };

  //
  // Render

  render() {
    const {
      name,
      implementation,
      ...otherProps
    } = this.props;

    return (
      <MenuItem
        {...otherProps}
        onPress={this.onPress}
      >
        {name}
      </MenuItem>
    );
  }
}

AddImportListPresetMenuItem.propTypes = {
  name: PropTypes.string.isRequired,
  implementation: PropTypes.string.isRequired,
  onPress: PropTypes.func.isRequired
};

export default AddImportListPresetMenuItem;
