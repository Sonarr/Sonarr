import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';

class LogsNavMenu extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isMenuOpen: false
    };
  }

  //
  // Listeners

  onMenuButtonPress = () => {
    this.setState({ isMenuOpen: !this.state.isMenuOpen });
  }

  onMenuItemPress = () => {
    this.setState({ isMenuOpen: false });
  }

  //
  // Render

  render() {
    const {
      current
    } = this.props;

    return (
      <Menu>
        <MenuButton
          onPress={this.onMenuButtonPress}
        >
          {current}
        </MenuButton>
        <MenuContent
          isOpen={this.state.isMenuOpen}
        >
          <MenuItem
            to={'/system/logs/files'}
          >
            Log Files
          </MenuItem>

          <MenuItem
            to={'/system/logs/files/update'}
          >
            Updater Log Files
          </MenuItem>
        </MenuContent>
      </Menu>
    );
  }
}

LogsNavMenu.propTypes = {
  current: PropTypes.string.isRequired
};

export default LogsNavMenu;
