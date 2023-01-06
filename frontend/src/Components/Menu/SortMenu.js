import PropTypes from 'prop-types';
import React from 'react';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';
import { align, icons } from 'Helpers/Props';

function SortMenu(props) {
  const {
    className,
    children,
    isDisabled,
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
        isDisabled={isDisabled}
      />
      {children}
    </Menu>
  );
}

SortMenu.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired,
  isDisabled: PropTypes.bool.isRequired,
  alignMenu: PropTypes.oneOf([align.LEFT, align.RIGHT])
};

SortMenu.defaultProps = {
  isDisabled: false
};

export default SortMenu;
