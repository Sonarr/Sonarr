import React, { useCallback } from 'react';
import TagInputConnector from './TagInputConnector';

interface SeriesTageInputProps {
  name: string;
  value: number | number[];
  onChange: ({
    name,
    value,
  }: {
    name: string;
    value: number | number[];
  }) => void;
}

export default function SeriesTagInput(props: SeriesTageInputProps) {
  const { value, onChange, ...otherProps } = props;
  const isArray = Array.isArray(value);

  const handleChange = useCallback(
    ({ name, value: newValue }: { name: string; value: number[] }) => {
      if (isArray) {
        onChange({ name, value: newValue });
      } else {
        onChange({
          name,
          value: newValue.length ? newValue[newValue.length - 1] : 0,
        });
      }
    },
    [isArray, onChange]
  );

  let finalValue: number[] = [];

  if (isArray) {
    finalValue = value;
  } else if (value === 0) {
    finalValue = [];
  } else {
    finalValue = [value];
  }

  return (
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore 2786 'TagInputConnector' isn't typed yet
    <TagInputConnector
      {...otherProps}
      value={finalValue}
      onChange={handleChange}
    />
  );
}
