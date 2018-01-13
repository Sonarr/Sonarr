import PropTypes from 'prop-types';
import React from 'react';
import SelectedMenuItem from './SelectedMenuItem';

function ViewMenuItem(props) {
  const {
    name,
    selectedView,
    ...otherProps
  } = props;

  const isSelected = name === selectedView;

  return (
    <SelectedMenuItem
      name={name}
      isSelected={isSelected}
      {...otherProps}
    />
  );
}

ViewMenuItem.propTypes = {
  name: PropTypes.string,
  selectedView: PropTypes.string.isRequired
};

export default ViewMenuItem;
