import React, { useCallback, useEffect } from 'react';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import useOAuth from 'OAuth/useOAuth';
import { InputOnChange } from 'typings/inputs';
import { getValidationFailures } from 'Utilities/selectSettings';
import { useFormInputGroup } from './FormInputGroupContext';

export interface OAuthInputProps {
  id?: string;
  label?: string;
  name: string;
  provider: string;
  providerData: Record<string, unknown>;
  section?: string;
  onChange: InputOnChange<unknown>;
}

function OAuthInput({
  id,
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
        id={id}
        aria-describedby={formInputActions?.describedBy}
        aria-invalid={formInputActions?.hasError || undefined}
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
