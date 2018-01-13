import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import keyboardShortcuts, { shortcuts } from 'Components/keyboardShortcuts';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SeriesSearchInputConnector from './SeriesSearchInputConnector';
import PageHeaderActionsMenuConnector from './PageHeaderActionsMenuConnector';
import KeyboardShortcutsModal from './KeyboardShortcutsModal';
import styles from './PageHeader.css';

class PageHeader extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props);

    this.state = {
      isKeyboardShortcutsModalOpen: false
    };
  }

  componentDidMount() {
    this.props.bindShortcut(shortcuts.OPEN_KEYBOARD_SHORTCUTS_MODAL.key, this.onOpenKeyboardShortcutsModal);
  }

  //
  // Control

  onOpenKeyboardShortcutsModal = () => {
    this.setState({ isKeyboardShortcutsModalOpen: true });
  }

  //
  // Listeners

  onKeyboardShortcutsModalClose = () => {
    this.setState({ isKeyboardShortcutsModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      onSidebarToggle
    } = this.props;

    return (
      <div className={styles.header}>
        <div className={styles.logoContainer}>
          <Link to={`${window.Sonarr.urlBase}/`}>
            <img
              className={styles.logo}
              src={`${window.Sonarr.urlBase}/Content/Images/logo.svg`}
            />
          </Link>
        </div>

        <div className={styles.sidebarToggleContainer}>
          <IconButton
            id="sidebar-toggle-button"
            name={icons.NAVBAR_COLLAPSE}
            onPress={onSidebarToggle}
          />
        </div>

        <SeriesSearchInputConnector />

        <div className={styles.right}>
          <IconButton
            className={styles.donate}
            name={icons.HEART}
            to="https://sonarr.tv/donate.html"
            size={14}
          />
          <PageHeaderActionsMenuConnector
            onKeyboardShortcutsPress={this.onOpenKeyboardShortcutsModal}
          />
        </div>

        <KeyboardShortcutsModal
          isOpen={this.state.isKeyboardShortcutsModalOpen}
          onModalClose={this.onKeyboardShortcutsModalClose}
        />
      </div>
    );
  }
}

PageHeader.propTypes = {
  onSidebarToggle: PropTypes.func.isRequired,
  bindShortcut: PropTypes.func.isRequired
};

export default keyboardShortcuts(PageHeader);
