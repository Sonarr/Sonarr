import React, { useCallback, useEffect, useRef, useState } from 'react';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { InputChanged } from 'typings/inputs';
import TextInput, { TextInputProps } from './TextInput';

function parseValue(
  value: string | null | undefined,
  isFloat: boolean,
  min: number | undefined,
  max: number | undefined
) {
  if (value == null || value === '') {
    return null;
  }

  let newValue = isFloat ? parseFloat(value) : parseInt(value);

  if (min != null && newValue != null && newValue < min) {
    newValue = min;
  } else if (max != null && newValue != null && newValue > max) {
    newValue = max;
  }

  return newValue;
}

export interface NumberInputProps
  extends Omit<TextInputProps, 'value' | 'onChange'> {
  value?: number | null;
  min?: number;
  max?: number;
  isFloat?: boolean;
  onChange: (input: InputChanged<number | null>) => void;
}

function NumberInput({
  name,
  value: inputValue = null,
  isFloat = false,
  min,
  max,
  onChange,
  ...otherProps
}: NumberInputProps) {
  const [value, setValue] = useState(
    inputValue == null ? '' : inputValue.toString()
  );
  const isFocused = useRef(false);
  const previousValue = usePrevious(inputValue);

  const handleChange = useCallback(
    ({ name, value: newValue }: InputChanged<string>) => {
      setValue(newValue);

      onChange({
        name,
        value: parseValue(newValue, isFloat, min, max),
      });
    },
    [isFloat, min, max, onChange, setValue]
  );

  const handleFocus = useCallback(() => {
    isFocused.current = true;
  }, []);

  const handleBlur = useCallback(() => {
    const parsedValue = parseValue(value, isFloat, min, max);
    const stringValue = parsedValue == null ? '' : parsedValue.toString();

    if (stringValue !== value) {
      setValue(stringValue);
    }

    onChange({
      name,
      value: parsedValue,
    });

    isFocused.current = false;
  }, [name, value, isFloat, min, max, onChange]);

  useEffect(() => {
    if (
      // @ts-expect-error inputValue may be null
      !isNaN(inputValue) &&
      inputValue !== previousValue &&
      !isFocused.current
    ) {
      setValue(inputValue == null ? '' : inputValue.toString());
    }
  }, [inputValue, previousValue, setValue]);

  return (
    <TextInput
      {...otherProps}
      name={name}
      type="number"
      value={value == null ? '' : value}
      min={min}
      max={max}
      onChange={handleChange}
      onBlur={handleBlur}
      onFocus={handleFocus}
    />
  );
}

export default NumberInput;
