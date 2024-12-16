import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { icons } from 'Helpers/Props';
import { setIsSidebarVisible } from 'Store/Actions/appActions';
import translate from 'Utilities/String/translate';
import KeyboardShortcutsModal from './KeyboardShortcutsModal';
import PageHeaderActionsMenu from './PageHeaderActionsMenu';
import SeriesSearchInput from './SeriesSearchInput';
import styles from './PageHeader.css';

function PageHeader() {
  const dispatch = useDispatch();

  const { isSidebarVisible } = useSelector((state: AppState) => state.app);

  const [isKeyboardShortcutsModalOpen, setIsKeyboardShortcutsModalOpen] =
    useState(false);

  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();

  const handleSidebarToggle = useCallback(() => {
    dispatch(setIsSidebarVisible({ isSidebarVisible: !isSidebarVisible }));
  }, [isSidebarVisible, dispatch]);

  const handleOpenKeyboardShortcutsModal = useCallback(() => {
    setIsKeyboardShortcutsModalOpen(true);
  }, []);

  const handleKeyboardShortcutsModalClose = useCallback(() => {
    setIsKeyboardShortcutsModalOpen(false);
  }, []);

  useEffect(() => {
    bindShortcut(
      'openKeyboardShortcutsModal',
      handleOpenKeyboardShortcutsModal
    );

    return () => {
      unbindShortcut('openKeyboardShortcutsModal');
    };
  }, [handleOpenKeyboardShortcutsModal, bindShortcut, unbindShortcut]);

  return (
    <div className={styles.header}>
      <div className={styles.logoContainer}>
        <Link className={styles.logoLink} to="/">
          <img
            className={styles.logo}
            src={`${window.Sonarr.urlBase}/Content/Images/logo.svg`}
            alt="Sonarr Logo"
          />
        </Link>
      </div>

      <div className={styles.sidebarToggleContainer}>
        <IconButton
          id="sidebar-toggle-button"
          name={icons.NAVBAR_COLLAPSE}
          onPress={handleSidebarToggle}
        />
      </div>

      <SeriesSearchInput />

      <div className={styles.right}>
        <IconButton
          className={styles.donate}
          name={icons.HEART}
          aria-label={translate('Donate')}
          to="https://sonarr.tv/donate.html"
          size={14}
          title={translate('Donate')}
        />

        <PageHeaderActionsMenu
          onKeyboardShortcutsPress={handleOpenKeyboardShortcutsModal}
        />
      </div>

      <KeyboardShortcutsModal
        isOpen={isKeyboardShortcutsModalOpen}
        onModalClose={handleKeyboardShortcutsModalClose}
      />
    </div>
  );
}

export default PageHeader;
