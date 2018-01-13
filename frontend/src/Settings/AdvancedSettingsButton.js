import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import styles from './AdvancedSettingsButton.css';

function AdvancedSettingsButton(props) {
  const {
    advancedSettings,
    onAdvancedSettingsPress
  } = props;

  return (
    <Link
      className={styles.button}
      title={advancedSettings ? 'Shown, click to hide' : 'Hidden, click to show'}
      onPress={onAdvancedSettingsPress}
    >
      <Icon
        name={icons.ADVANCED_SETTINGS}
        size={21}
      />

      <span
        className={classNames(
          styles.indicatorContainer,
          'fa-layers fa-fw'
        )}
      >
        <Icon
          className={styles.indicatorBackground}
          name={icons.CIRCLE}
          size={16}
        />

        <Icon
          className={advancedSettings ? styles.enabled : styles.disabled}
          name={advancedSettings ? icons.CHECK : icons.CLOSE}
          size={10}
        />
      </span>

      <div className={styles.labelContainer}>
        <div className={styles.label}>
          {advancedSettings ? 'Hide Advanced' : 'Show Advanced'}
        </div>
      </div>
    </Link>
  );
}

AdvancedSettingsButton.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  onAdvancedSettingsPress: PropTypes.func.isRequired
};

export default AdvancedSettingsButton;
