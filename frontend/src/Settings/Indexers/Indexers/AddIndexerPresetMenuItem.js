import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MenuItem from 'Components/Menu/MenuItem';

class AddIndexerPresetMenuItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      name,
      implementation,
      implementationName
    } = this.props;

    this.props.onPress({
      name,
      implementation,
      implementationName
    });
  };

  //
  // Render

  render() {
    const {
      name,
      implementation,
      implementationName,
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

AddIndexerPresetMenuItem.propTypes = {
  name: PropTypes.string.isRequired,
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  onPress: PropTypes.func.isRequired
};

export default AddIndexerPresetMenuItem;
