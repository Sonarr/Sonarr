import React, { useCallback, useEffect, useState } from 'react';
import FormInputButton from 'Components/Form/FormInputButton';
import Icon from 'Components/Icon';
import { icons, kinds } from 'Helpers/Props';
import { ButtonProps } from './Button';
import styles from './ClipboardButton.css';

export interface ClipboardButtonProps extends Omit<ButtonProps, 'children'> {
  value: string;
}

export type ClipboardState = 'success' | 'error' | null;

export default function ClipboardButton({
  id,
  value,
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
      await navigator.clipboard.writeText(value);
      setState('success');
    } catch (_) {
      setState('error');
    }
  }, [value]);

  return (
    <FormInputButton
      className={className}
      onClick={handleClick}
      {...otherProps}
    >
      <span className={state ? styles.showStateIcon : undefined}>
        {state ? (
          <span className={styles.stateIconContainer}>
            <Icon
              name={state === 'error' ? icons.DANGER : icons.CHECK}
              kind={state === 'error' ? kinds.DANGER : kinds.SUCCESS}
            />
          </span>
        ) : null}

        <span className={styles.clipboardIconContainer}>
          <Icon name={icons.CLIPBOARD} />
        </span>
      </span>
    </FormInputButton>
  );
}
