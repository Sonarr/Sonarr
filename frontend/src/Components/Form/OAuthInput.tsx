import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import { kinds } from 'Helpers/Props';
import { resetOAuth, startOAuth } from 'Store/Actions/oAuthActions';
import { InputOnChange } from 'typings/inputs';

export interface OAuthInputProps {
  label?: string;
  name: string;
  provider: string;
  providerData: object;
  section: string;
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
  const dispatch = useDispatch();
  const { authorizing, error, result } = useSelector(
    (state: AppState) => state.oAuth
  );

  const handlePress = useCallback(() => {
    dispatch(
      startOAuth({
        name,
        provider,
        providerData,
        section,
      })
    );
  }, [name, provider, providerData, section, dispatch]);

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
      dispatch(resetOAuth());
    };
  }, [dispatch]);

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
