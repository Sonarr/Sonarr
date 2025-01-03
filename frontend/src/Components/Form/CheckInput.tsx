import classNames from 'classnames';
import React, { SyntheticEvent, useCallback, useEffect, useRef } from 'react';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { CheckInputChanged } from 'typings/inputs';
import FormInputHelpText from './FormInputHelpText';
import styles from './CheckInput.css';

interface ChangeEvent<T = Element> extends SyntheticEvent<T, MouseEvent> {
  target: EventTarget & T;
}

export interface CheckInputProps {
  className?: string;
  containerClassName?: string;
  name: string;
  checkedValue?: boolean;
  uncheckedValue?: boolean;
  value?: string | boolean | null;
  helpText?: string;
  helpTextWarning?: string;
  isDisabled?: boolean;
  kind?: Extract<Kind, keyof typeof styles>;
  onChange: (changes: CheckInputChanged) => void;
}

function CheckInput(props: CheckInputProps) {
  const {
    className = styles.input,
    containerClassName = styles.container,
    name,
    value,
    checkedValue = true,
    uncheckedValue = false,
    helpText,
    helpTextWarning,
    isDisabled,
    kind = 'primary',
    onChange,
  } = props;

  const inputRef = useRef<HTMLInputElement>(null);

  const isChecked = value === checkedValue;
  const isUnchecked = value === uncheckedValue;
  const isIndeterminate = !isChecked && !isUnchecked;

  const toggleChecked = useCallback(
    (checked: boolean, shiftKey: boolean) => {
      const newValue = checked ? checkedValue : uncheckedValue;

      if (value !== newValue) {
        onChange({
          name,
          value: newValue,
          shiftKey,
        });
      }
    },
    [name, value, checkedValue, uncheckedValue, onChange]
  );

  const handleClick = useCallback(
    (event: SyntheticEvent<HTMLElement, MouseEvent>) => {
      if (isDisabled) {
        return;
      }

      const shiftKey = event.nativeEvent.shiftKey;
      const checked = !(inputRef.current?.checked ?? false);

      event.preventDefault();
      toggleChecked(checked, shiftKey);
    },
    [isDisabled, toggleChecked]
  );

  const handleChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      const checked = event.target.checked;
      const shiftKey = event.nativeEvent.shiftKey;

      toggleChecked(checked, shiftKey);
    },
    [toggleChecked]
  );

  useEffect(() => {
    if (!inputRef.current) {
      return;
    }

    inputRef.current.indeterminate =
      value !== uncheckedValue && value !== checkedValue;
  }, [value, uncheckedValue, checkedValue]);

  return (
    <div className={containerClassName}>
      <label className={styles.label} onClick={handleClick}>
        <input
          ref={inputRef}
          className={styles.checkbox}
          type="checkbox"
          name={name}
          checked={isChecked}
          disabled={isDisabled}
          onChange={handleChange}
        />

        <div
          className={classNames(
            className,
            isChecked ? styles[kind] : styles.isNotChecked,
            isIndeterminate && styles.isIndeterminate,
            isDisabled && styles.isDisabled
          )}
        >
          {isChecked ? <Icon name={icons.CHECK} /> : null}

          {isIndeterminate ? <Icon name={icons.CHECK_INDETERMINATE} /> : null}
        </div>

        {helpText ? (
          <FormInputHelpText className={styles.helpText} text={helpText} />
        ) : null}

        {!helpText && helpTextWarning ? (
          <FormInputHelpText
            className={styles.helpText}
            text={helpTextWarning}
            isWarning={true}
          />
        ) : null}
      </label>
    </div>
  );
}

export default CheckInput;
