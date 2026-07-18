import React from 'react';
import IconButton, { IconButtonProps } from 'Components/Link/IconButton';
import styles from './SettingsCard.css';

function SettingsCardAction(props: IconButtonProps) {
  return <IconButton className={styles.actionButton} {...props} />;
}

export default SettingsCardAction;
