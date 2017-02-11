import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';

function SortMenu(props) {
  const {
    className,
    children,
    ...otherProps
  } = props;

  return (
    <Menu
      className={className}
      {...otherProps}
    >
      <ToolbarMenuButton
        iconName={icons.SORT}
        text="Sort"
      />
      {children}
    </Menu>
  );
}

SortMenu.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired
};

export default SortMenu;
