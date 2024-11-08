import React, { useCallback, useMemo } from 'react';
import { useSelector } from 'react-redux';
import Language from 'Language/Language';
import createFilteredLanguagesSelector from 'Store/Selectors/createFilteredLanguagesSelector';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

interface LanguageSelectInputOnChangeProps {
  name: string;
  value: number | string | Language;
}

interface LanguageSelectInputProps {
  name: string;
  value: number | string | Language;
  includeNoChange: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed: boolean;
  onChange: (payload: LanguageSelectInputOnChangeProps) => void;
}

export default function LanguageSelectInput({
  value,
  includeNoChange,
  includeNoChangeDisabled,
  includeMixed,
  onChange,
  ...otherProps
}: LanguageSelectInputProps) {
  const { items } = useSelector(createFilteredLanguagesSelector(true));

  const values = useMemo(() => {
    const result: EnhancedSelectInputValue<number | string>[] = items.map(
      (item) => {
        return {
          key: item.id,
          value: item.name,
        };
      }
    );

    if (includeNoChange) {
      result.unshift({
        key: 'noChange',
        value: translate('NoChange'),
        isDisabled: includeNoChangeDisabled,
      });
    }

    if (includeMixed) {
      result.unshift({
        key: 'mixed',
        value: `(${translate('Mixed')})`,
        isDisabled: true,
      });
    }

    return result;
  }, [includeNoChange, includeNoChangeDisabled, includeMixed, items]);

  const selectValue =
    typeof value === 'number' || typeof value === 'string' ? value : value.id;

  const handleChange = useCallback(
    (payload: LanguageSelectInputOnChangeProps) => {
      if (typeof value === 'number') {
        onChange(payload);
      } else {
        const language = items.find((i) => i.id === payload.value);

        onChange({
          ...payload,
          value: language
            ? {
                id: language.id,
                name: language.name,
              }
            : ({ id: payload.value } as Language),
        });
      }
    },
    [value, items, onChange]
  );

  return (
    <EnhancedSelectInput
      {...otherProps}
      value={selectValue}
      values={values}
      onChange={handleChange}
    />
  );
}
