import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import ReCAPTCHA from 'react-google-recaptcha';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import FormInputButton from './FormInputButton';
import TextInput from './TextInput';
import styles from './CaptchaInput.css';

function CaptchaInput(props) {
  const {
    className,
    name,
    value,
    hasError,
    hasWarning,
    refreshing,
    siteKey,
    secretToken,
    onChange,
    onRefreshPress,
    onCaptchaChange
  } = props;

  return (
    <div>
      <div className={styles.captchaInputWrapper}>
        <TextInput
          className={classNames(
            className,
            styles.hasButton,
            hasError && styles.hasError,
            hasWarning && styles.hasWarning
          )}
          name={name}
          value={value}
          onChange={onChange}
        />

        <FormInputButton
          onPress={onRefreshPress}
        >
          <Icon
            name={icons.REFRESH}
            isSpinning={refreshing}
          />
        </FormInputButton>
      </div>

      {
        !!siteKey && !!secretToken &&
          <div className={styles.recaptchaWrapper}>
            <ReCAPTCHA
              sitekey={siteKey}
              stoken={secretToken}
              onChange={onCaptchaChange}
            />
          </div>
      }
    </div>
  );
}

CaptchaInput.propTypes = {
  className: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  refreshing: PropTypes.bool.isRequired,
  siteKey: PropTypes.string,
  secretToken: PropTypes.string,
  onChange: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onCaptchaChange: PropTypes.func.isRequired
};

CaptchaInput.defaultProps = {
  className: styles.input,
  value: ''
};

export default CaptchaInput;
