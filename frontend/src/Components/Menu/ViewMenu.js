import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';

function ViewMenu(props) {
  const {
    children,
    ...otherProps
  } = props;

  return (
    <Menu
      {...otherProps}
    >
      <ToolbarMenuButton
        iconName={icons.VIEW}
        text="View"
      />
      {children}
    </Menu>
  );
}

ViewMenu.propTypes = {
  children: PropTypes.node.isRequired
};

export default ViewMenu;
