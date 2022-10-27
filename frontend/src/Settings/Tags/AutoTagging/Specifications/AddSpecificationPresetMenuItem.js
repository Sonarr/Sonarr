import PropTypes from 'prop-types';
import React, { useCallback } from 'react';
import MenuItem from 'Components/Menu/MenuItem';

export default function AddSpecificationPresetMenuItem(props) {
  const {
    name,
    implementation,
    onPress,
    ...otherProps
  } = props;

  const onWrappedPress = useCallback(() => {
    onPress({
      name,
      implementation
    });
  }, [name, implementation, onPress]);

  return (
    <MenuItem
      {...otherProps}
      onPress={onWrappedPress}
    >
      {name}
    </MenuItem>
  );
}

AddSpecificationPresetMenuItem.propTypes = {
  name: PropTypes.string.isRequired,
  implementation: PropTypes.string.isRequired,
  onPress: PropTypes.func.isRequired
};
