import PropTypes from 'prop-types';
import React from 'react';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';
import { align, icons } from 'Helpers/Props';

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
  isDisabled: PropTypes.bool.isRequired,
  alignMenu: PropTypes.oneOf([align.LEFT, align.RIGHT])
};

ViewMenu.defaultProps = {
  isDisabled: false
};

export default ViewMenu;
