import classNames from 'classnames';
import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import {
  toggleShowAdvancedSettings,
  useShowAdvancedSettings,
} from './advancedSettingsStore';
import styles from './AdvancedSettingsButton.css';

interface AdvancedSettingsButtonProps {
  showLabel: boolean;
}

function AdvancedSettingsButton({ showLabel }: AdvancedSettingsButtonProps) {
  const showAdvancedSettings = useShowAdvancedSettings();

  const handlePress = useCallback(() => {
    toggleShowAdvancedSettings();
  }, []);

  return (
    <Link
      className={styles.button}
      title={
        showAdvancedSettings
          ? translate('ShownClickToHide')
          : translate('HiddenClickToShow')
      }
      onPress={handlePress}
    >
      <Icon name={icons.ADVANCED_SETTINGS} size={21} />

      <span
        className={classNames(styles.indicatorContainer, 'fa-layers fa-fw')}
      >
        <Icon
          className={styles.indicatorBackground}
          name={icons.CIRCLE}
          size={16}
        />

        <Icon
          className={showAdvancedSettings ? styles.enabled : styles.disabled}
          name={showAdvancedSettings ? icons.CHECK : icons.CLOSE}
          size={10}
        />
      </span>

      {showLabel ? (
        <div className={styles.labelContainer}>
          <div className={styles.label}>
            {showAdvancedSettings
              ? translate('HideAdvanced')
              : translate('ShowAdvanced')}
          </div>
        </div>
      ) : null}
    </Link>
  );
}

export default AdvancedSettingsButton;
