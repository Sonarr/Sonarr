import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { InputOnChange } from 'typings/inputs';
import KeyValueListInputItem from './KeyValueListInputItem';
import styles from './KeyValueListInput.css';

interface KeyValue {
  key: string;
  value: string;
}

export interface KeyValueListInputProps {
  className?: string;
  name: string;
  value: KeyValue[];
  hasError?: boolean;
  hasWarning?: boolean;
  keyPlaceholder?: string;
  valuePlaceholder?: string;
  onChange: InputOnChange<KeyValue[]>;
}

function KeyValueListInput({
  className = styles.inputContainer,
  name,
  value = [],
  hasError = false,
  hasWarning = false,
  keyPlaceholder,
  valuePlaceholder,
  onChange,
}: KeyValueListInputProps): JSX.Element {
  const [isFocused, setIsFocused] = useState(false);

  const handleItemChange = useCallback(
    (index: number | null, itemValue: KeyValue) => {
      const newValue = [...value];

      if (index === null) {
        newValue.push(itemValue);
      } else {
        newValue.splice(index, 1, itemValue);
      }

      onChange({ name, value: newValue });
    },
    [value, name, onChange]
  );

  const handleRemoveItem = useCallback(
    (index: number) => {
      const newValue = [...value];
      newValue.splice(index, 1);
      onChange({ name, value: newValue });
    },
    [value, name, onChange]
  );

  const onFocus = useCallback(() => setIsFocused(true), []);

  const onBlur = useCallback(() => {
    setIsFocused(false);

    const newValue = value.reduce((acc: KeyValue[], v) => {
      if (v.key || v.value) {
        acc.push(v);
      }
      return acc;
    }, []);

    if (newValue.length !== value.length) {
      onChange({ name, value: newValue });
    }
  }, [value, name, onChange]);

  return (
    <div
      className={classNames(
        className,
        isFocused && styles.isFocused,
        hasError && styles.hasError,
        hasWarning && styles.hasWarning
      )}
    >
      {[...value, { key: '', value: '' }].map((v, index) => (
        <KeyValueListInputItem
          key={index}
          index={index}
          keyValue={v.key}
          value={v.value}
          keyPlaceholder={keyPlaceholder}
          valuePlaceholder={valuePlaceholder}
          isNew={index === value.length}
          onChange={handleItemChange}
          onRemove={handleRemoveItem}
          onFocus={onFocus}
          onBlur={onBlur}
        />
      ))}
    </div>
  );
}

export default KeyValueListInput;
