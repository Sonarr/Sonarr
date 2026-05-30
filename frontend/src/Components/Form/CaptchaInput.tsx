import classNames from 'classnames';
import React, { useCallback, useEffect } from 'react';
import ReCAPTCHA from 'react-google-recaptcha';
import Icon from 'Components/Icon';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import FormInputButton from './FormInputButton';
import TextInput from './TextInput';
import useCaptcha from './useCaptcha';
import styles from './CaptchaInput.css';

export interface CaptchaInputProps {
  className?: string;
  name: string;
  value?: string;
  provider: string;
  providerData: object;
  hasError?: boolean;
  hasWarning?: boolean;
  onChange: (change: InputChanged<string>) => unknown;
}

function CaptchaInput({
  className = styles.input,
  name,
  value = '',
  provider,
  providerData,
  hasError,
  hasWarning,
  onChange,
}: CaptchaInputProps) {
  const {
    token,
    refreshing,
    siteKey,
    secretToken,
    refresh,
    getCaptchaCookie,
    reset,
  } = useCaptcha();
  const previousToken = usePrevious(token);

  const handleCaptchaChange = useCallback(
    (captchaResponse: string | null) => {
      // If the captcha has expired `captchaResponse` will be null.
      // In the event it's null don't try to get the captchaCookie.
      // TODO: Should we clear the cookie? or reset the captcha?

      if (!captchaResponse) {
        return;
      }

      getCaptchaCookie({
        provider,
        providerData,
        captchaResponse,
      });
    },
    [provider, providerData, getCaptchaCookie]
  );

  const handleRefreshPress = useCallback(() => {
    refresh({ provider, providerData });
  }, [provider, providerData, refresh]);

  useEffect(() => {
    if (token && token !== previousToken) {
      onChange({ name, value: token });
    }
  }, [name, token, previousToken, onChange]);

  useEffect(() => {
    reset();
  }, [reset]);

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

        <FormInputButton onPress={handleRefreshPress}>
          <Icon name={icons.REFRESH} isSpinning={refreshing} />
        </FormInputButton>
      </div>

      {siteKey && secretToken ? (
        <div className={styles.recaptchaWrapper}>
          <ReCAPTCHA
            sitekey={siteKey}
            stoken={secretToken}
            onChange={handleCaptchaChange}
          />
        </div>
      ) : null}
    </div>
  );
}

export default CaptchaInput;
