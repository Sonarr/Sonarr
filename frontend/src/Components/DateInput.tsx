import React, { useCallback, useRef } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import { isCrossOriginFrame } from 'Utilities/browser';
import styles from './DateInput.css';

const hasDatePicker =
  window.matchMedia('(pointer: coarse)').matches ||
  ('showPicker' in HTMLInputElement.prototype && !isCrossOriginFrame());

interface DateInputProps {
  className?: string;
  buttonClassName?: string;
  value: string;
  label: string;
  isDisabled?: boolean;
  onChange: (value: string) => void;
}

function DateInput({
  className = styles.dateInput,
  buttonClassName = styles.button,
  value,
  label,
  isDisabled = false,
  onChange,
}: DateInputProps) {
  const inputRef = useRef<HTMLInputElement>(null);

  const handlePress = useCallback(() => {
    inputRef.current?.showPicker();
  }, []);

  const handleChange = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>) => {
      const { value: newValue } = event.target;

      if (newValue) {
        onChange(newValue);
      }
    },
    [onChange]
  );

  if (!hasDatePicker) {
    return null;
  }

  return (
    <span className={className}>
      <Link
        className={buttonClassName}
        isDisabled={isDisabled}
        aria-label={label}
        title={label}
        onPress={handlePress}
      >
        <Icon name={icons.CALENDAR_O} />
      </Link>

      <input
        ref={inputRef}
        type="date"
        className={styles.input}
        value={value}
        aria-label={label}
        tabIndex={-1}
        onChange={handleChange}
      />
    </span>
  );
}

export default DateInput;
