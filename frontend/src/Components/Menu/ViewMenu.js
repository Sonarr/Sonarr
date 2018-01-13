import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';

function ViewMenu(props) {
  const {
    children,
    isDisabled,
    ...otherProps
  } = props;

  return (
    <Menu
      {...otherProps}
    >
      <ToolbarMenuButton
        iconName={icons.VIEW}
        text="View"
        isDisabled={isDisabled}
      />
      {children}
    </Menu>
  );
}

ViewMenu.propTypes = {
  children: PropTypes.node.isRequired,
  isDisabled: PropTypes.bool.isRequired
};

ViewMenu.defaultProps = {
  isDisabled: false
};

export default ViewMenu;
