import React, { useMemo } from 'react';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import EnhancedSelectInput, {
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

interface LanguageSelectInputProps {
  name: string;
  value: number;
  values: EnhancedSelectInputValue<number>[];
  onChange: (change: EnhancedSelectInputChanged<number>) => void;
}

function LanguageSelectInput({
  values,
  onChange,
  ...otherProps
}: LanguageSelectInputProps) {
  const mappedValues = useMemo(() => {
    const minId = values.reduce(
      (min: number, v) => (v.key < 1 ? v.key : min),
      values[0].key
    );

    return values.map(({ key, value }) => {
      return {
        key,
        value,
        dividerAfter: minId < 1 ? key === minId : false,
      };
    });
  }, [values]);

  return (
    <EnhancedSelectInput
      {...otherProps}
      values={mappedValues}
      onChange={onChange}
    />
  );
}

export default LanguageSelectInput;
