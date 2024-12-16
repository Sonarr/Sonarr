import React from 'react';
import { useSelector } from 'react-redux';
import { Shortcut, shortcuts } from 'Components/keyboardShortcuts';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import translate from 'Utilities/String/translate';
import styles from './KeyboardShortcutsModalContent.css';

function getShortcuts() {
  const allShortcuts: Shortcut[] = [];

  Object.keys(shortcuts).forEach((key) => {
    allShortcuts.push(shortcuts[key]);
  });

  return allShortcuts;
}

function getShortcutKey(combo: string, isOsx: boolean) {
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

interface KeyboardShortcutsModalContentProps {
  onModalClose: () => void;
}

function KeyboardShortcutsModalContent({
  onModalClose,
}: KeyboardShortcutsModalContentProps) {
  const { isOsx } = useSelector(createSystemStatusSelector());
  const allShortcuts = getShortcuts();

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('KeyboardShortcuts')}</ModalHeader>

      <ModalBody>
        {allShortcuts.map((shortcut) => {
          return (
            <div key={shortcut.name} className={styles.shortcut}>
              <div className={styles.key}>
                {getShortcutKey(shortcut.key, isOsx)}
              </div>

              <div>{shortcut.name}</div>
            </div>
          );
        })}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default KeyboardShortcutsModalContent;
