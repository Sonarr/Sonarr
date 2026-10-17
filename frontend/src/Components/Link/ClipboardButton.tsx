import copy from 'copy-to-clipboard';
import React, { useCallback, useEffect, useState } from 'react';
import FormInputButton from 'Components/Form/FormInputButton';
import Icon from 'Components/Icon';
import StatusIndicator from 'Components/StatusIndicator';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import { ButtonProps } from './Button';
import styles from './ClipboardButton.module.css';

export interface ClipboardButtonProps extends Omit<ButtonProps, 'children'> {
  value: string;
  label?: string | number;
}

export type ClipboardState = 'success' | 'error' | null;

export default function ClipboardButton({
  id,
  value,
  label,
  title = translate('CopyToClipboard'),
  className = styles.button,
  ...otherProps
}: ClipboardButtonProps) {
  const [state, setState] = useState<ClipboardState>(null);

  useEffect(() => {
    if (!state) {
      return;
    }

    const timeoutId = setTimeout(() => {
      setState(null);
    }, 3000);

    return () => {
      if (timeoutId) {
        clearTimeout(timeoutId);
      }
    };
  }, [state]);

  const handleClick = useCallback(async () => {
    try {
      if ('clipboard' in navigator) {
        await navigator.clipboard.writeText(value);
      } else {
        copy(value);
      }

      setState('success');
    } catch (e) {
      setState('error');
      console.error(`Failed to copy to clipboard`, e);
    }
  }, [value]);

  const isError = state === 'error';
  const clipboardIcon = <Icon name={icons.CLIPBOARD} aria-hidden={true} />;

  const stateIndicator = state ? (
    <StatusIndicator
      className={label ? undefined : styles.stateIconContainer}
      label={translate(isError ? 'CopyToClipboardError' : 'CopiedToClipboard')}
      role={isError ? 'alert' : 'status'}
      aria-atomic={true}
    >
      <Icon
        name={isError ? icons.DANGER : icons.CHECK}
        kind={isError ? kinds.DANGER : kinds.SUCCESS}
        aria-hidden={true}
      />
    </StatusIndicator>
  ) : null;

  return (
    <FormInputButton
      className={className}
      title={title}
      aria-label={title}
      onClick={handleClick}
      {...otherProps}
    >
      {label ? (
        <>
          <span className={styles.buttonText}>{label}</span>

          <span className={styles.labelIconContainer}>
            {stateIndicator ?? clipboardIcon}
          </span>
        </>
      ) : (
        <span className={state ? styles.showStateIcon : undefined}>
          {stateIndicator}

          <span className={styles.clipboardIconContainer}>{clipboardIcon}</span>
        </span>
      )}
    </FormInputButton>
  );
}
