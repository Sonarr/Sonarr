import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import MenuItem from './MenuItem';
import styles from './SelectedMenuItem.css';

class SelectedMenuItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      name,
      onPress
    } = this.props;

    onPress(name);
  }

  //
  // Render

  render() {
    const {
      children,
      selectedIconName,
      isSelected,
      ...otherProps
    } = this.props;

    return (
      <MenuItem
        {...otherProps}
        onPress={this.onPress}
      >
        <div className={styles.item}>
          {children}

          <Icon
            className={isSelected ? styles.isSelected : styles.isNotSelected}
            name={selectedIconName}
          />
        </div>
      </MenuItem>
    );
  }
}

SelectedMenuItem.propTypes = {
  name: PropTypes.string,
  children: PropTypes.node.isRequired,
  selectedIconName: PropTypes.object.isRequired,
  isSelected: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

SelectedMenuItem.defaultProps = {
  selectedIconName: icons.CHECK
};

export default SelectedMenuItem;
