import React from 'react';
import IconButton from 'Components/Link/IconButton';
import { LinkProps } from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './TableOptionsButton.module.css';

function TableOptionsButton({ onPress }: Pick<LinkProps, 'onPress'>) {
  return (
    <IconButton
      className={styles.button}
      name={icons.ADVANCED_SETTINGS}
      size={14}
      title={translate('TableOptions')}
      aria-label={translate('TableOptions')}
      onPress={onPress}
    />
  );
}

export default TableOptionsButton;
