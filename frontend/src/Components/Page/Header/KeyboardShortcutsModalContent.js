import PropTypes from 'prop-types';
import React from 'react';
import { shortcuts } from 'Components/keyboardShortcuts';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import styles from './KeyboardShortcutsModalContent.css';

function getShortcuts() {
  const allShortcuts = [];

  Object.keys(shortcuts).forEach((key) => {
    allShortcuts.push(shortcuts[key]);
  });

  return allShortcuts;
}

function getShortcutKey(combo, isOsx) {
  const comboMatch = combo.match(/(.+?)\+(.)/);

  if (!comboMatch) {
    return combo;
  }

  const modifier = comboMatch[1];
  const key = comboMatch[2];
  let osModifier = modifier;

  if (modifier === 'mod') {
    osModifier = isOsx ? 'cmd' : 'ctrl';
  }

  return `${osModifier} + ${key}`;
}

function KeyboardShortcutsModalContent(props) {
  const {
    isOsx,
    onModalClose
  } = props;

  const allShortcuts = getShortcuts();

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Keyboard Shortcuts
      </ModalHeader>

      <ModalBody>
        {
          allShortcuts.map((shortcut) => {
            return (
              <div
                key={shortcut.name}
                className={styles.shortcut}
              >
                <div className={styles.key}>
                  {getShortcutKey(shortcut.key, isOsx)}
                </div>

                <div>
                  {shortcut.name}
                </div>
              </div>
            );
          })
        }
      </ModalBody>

      <ModalFooter>
        <Button
          onPress={onModalClose}
        >
          Close
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

KeyboardShortcutsModalContent.propTypes = {
  isOsx: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default KeyboardShortcutsModalContent;
