import React, { useCallback, useEffect } from 'react';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import { kinds } from 'Helpers/Props';
import useOAuth from 'OAuth/useOAuth';
import { getValidationFailures } from 'Store/Selectors/selectSettings';
import { InputOnChange } from 'typings/inputs';
import { useFormInputGroup } from './FormInputGroupContext';

export interface OAuthInputProps {
  label?: string;
  name: string;
  provider: string;
  providerData: Record<string, unknown>;
  section?: string;
  onChange: InputOnChange<unknown>;
}

function OAuthInput({
  label = 'Start OAuth',
  name,
  provider,
  providerData,
  section,
  onChange,
}: OAuthInputProps) {
  const formInputActions = useFormInputGroup();
  const { authorizing, error, result, startOAuth, resetOAuth } = useOAuth();

  const handlePress = useCallback(() => {
    startOAuth({
      name,
      provider,
      providerData,
      section,
    });
  }, [name, provider, providerData, section, startOAuth]);

  useEffect(() => {
    if (!result) {
      return;
    }

    Object.keys(result).forEach((key) => {
      onChange({ name: key, value: result[key] });
    });
  }, [result, onChange]);

  useEffect(() => {
    return () => {
      resetOAuth();
    };
  }, [resetOAuth]);

  useEffect(() => {
    const validationFailures = getValidationFailures(error);

    formInputActions?.setClientErrors(validationFailures?.errors ?? []);
    formInputActions?.setClientWarnings(validationFailures?.warnings ?? []);
  }, [name, error, formInputActions]);

  return (
    <div>
      <SpinnerErrorButton
        kind={kinds.PRIMARY}
        isSpinning={authorizing}
        error={error}
        onPress={handlePress}
      >
        {label}
      </SpinnerErrorButton>
    </div>
  );
}

export default OAuthInput;
