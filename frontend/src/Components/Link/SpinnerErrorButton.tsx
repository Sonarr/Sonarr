import React, { useEffect, useMemo, useRef, useState } from 'react';
import { Error } from 'App/State/AppSectionState';
import Icon, { IconKind, IconName } from 'Components/Icon';
import SpinnerButton, {
  SpinnerButtonProps,
} from 'Components/Link/SpinnerButton';
import { getValidationFailures } from 'Helpers/Hooks/useApiMutation';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons } from 'Helpers/Props';
import { ValidationFailure } from 'typings/pending';
import { ApiError } from 'Utilities/Fetch/fetchJson';
import styles from './SpinnerErrorButton.css';

function getTestResult(error: ApiError | Error | string | undefined | null) {
  if (!error) {
    return {
      wasSuccessful: true,
      hasWarning: false,
      hasError: false,
    };
  }

  if (typeof error === 'string') {
    return {
      wasSuccessful: false,
      hasWarning: false,
      hasError: true,
    };
  }

  if ('status' in error) {
    if (error.status !== 400) {
      return {
        wasSuccessful: false,
        hasWarning: false,
        hasError: true,
      };
    }

    const failures = error.responseJSON as ValidationFailure[];

    const { hasError, hasWarning } = failures.reduce(
      (acc, failure) => {
        if (failure.isWarning) {
          acc.hasWarning = true;
        } else {
          acc.hasError = true;
        }

        return acc;
      },
      { hasWarning: false, hasError: false }
    );

    return {
      wasSuccessful: false,
      hasWarning,
      hasError,
    };
  } else if ('statusCode' in error) {
    if (error.statusCode !== 400 || error.statusBody == null) {
      return {
        wasSuccessful: false,
        hasWarning: false,
        hasError: true,
      };
    }

    const failures = getValidationFailures(error);

    return {
      wasSuccessful: false,
      hasWarning: failures.warnings.length > 0,
      hasError: failures.errors.length > 0,
    };
  }

  return {
    wasSuccessful: false,
    hasWarning: false,
    hasError: true,
  };
}

interface SpinnerErrorButtonProps extends SpinnerButtonProps {
  isSpinning: boolean;
  error?: ApiError | Error | string | null;
  children: React.ReactNode;
}

function SpinnerErrorButton({
  kind,
  isSpinning,
  error,
  children,
  ...otherProps
}: SpinnerErrorButtonProps) {
  const wasSpinning = usePrevious(isSpinning);
  const updateTimeout = useRef<ReturnType<typeof setTimeout>>();

  const [result, setResult] = useState({
    wasSuccessful: false,
    hasWarning: false,
    hasError: false,
  });
  const { wasSuccessful, hasWarning, hasError } = result;

  const showIcon = wasSuccessful || hasWarning || hasError;

  const { iconName, iconKind } = useMemo<{
    iconName: IconName;
    iconKind: IconKind;
  }>(() => {
    if (hasWarning) {
      return {
        iconName: icons.WARNING,
        iconKind: 'warning',
      };
    }

    if (hasError) {
      return {
        iconName: icons.DANGER,
        iconKind: 'danger',
      };
    }

    return {
      iconName: icons.CHECK,
      iconKind: kind === 'primary' ? 'default' : 'success',
    };
  }, [kind, hasError, hasWarning]);

  useEffect(() => {
    if (wasSpinning && !isSpinning) {
      const testResult = getTestResult(error);

      setResult(testResult);

      const { wasSuccessful, hasWarning, hasError } = testResult;

      if (wasSuccessful || hasWarning || hasError) {
        updateTimeout.current = setTimeout(() => {
          setResult({
            wasSuccessful: false,
            hasWarning: false,
            hasError: false,
          });
        }, 3000);
      }
    }
  }, [isSpinning, wasSpinning, error]);

  useEffect(() => {
    return () => {
      if (updateTimeout.current) {
        clearTimeout(updateTimeout.current);
      }
    };
  }, []);

  return (
    <SpinnerButton kind={kind} isSpinning={isSpinning} {...otherProps}>
      <span className={showIcon ? styles.showIcon : undefined}>
        {showIcon && (
          <span className={styles.iconContainer}>
            <Icon name={iconName} kind={iconKind} />
          </span>
        )}

        <span className={styles.label}>{children}</span>
      </span>
    </SpinnerButton>
  );
}

export default SpinnerErrorButton;
