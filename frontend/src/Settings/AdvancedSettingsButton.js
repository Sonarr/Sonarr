import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './AdvancedSettingsButton.css';

function AdvancedSettingsButton(props) {
  const {
    advancedSettings,
    onAdvancedSettingsPress,
    showLabel
  } = props;

  return (
    <Link
      className={styles.button}
      title={advancedSettings ? translate('ShownClickToHide') : translate('HiddenClickToShow')}
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

      {
        showLabel ?
          <div className={styles.labelContainer}>
            <div className={styles.label}>
              {advancedSettings ? translate('HideAdvanced') : translate('ShowAdvanced')}
            </div>
          </div> :
          null
      }
    </Link>
  );
}

AdvancedSettingsButton.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  onAdvancedSettingsPress: PropTypes.func.isRequired,
  showLabel: PropTypes.bool.isRequired
};

AdvancedSettingsButton.defaultProps = {
  showLabel: true
};

export default AdvancedSettingsButton;
