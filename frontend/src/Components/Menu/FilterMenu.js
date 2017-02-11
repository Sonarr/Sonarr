import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';
import styles from './FilterMenu.css';

function FilterMenu(props) {
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
        iconName={icons.FILTER}
        text="Filter"
      />
      {children}
    </Menu>
  );
}

FilterMenu.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired
};

FilterMenu.defaultProps = {
  className: styles.filterMenu
};

export default FilterMenu;
