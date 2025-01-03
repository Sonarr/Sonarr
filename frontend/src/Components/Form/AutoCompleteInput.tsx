import jdu from 'jdu';
import React, { SyntheticEvent, useCallback, useState } from 'react';
import {
  ChangeEvent,
  SuggestionsFetchRequestedParams,
} from 'react-autosuggest';
import { InputChanged } from 'typings/inputs';
import AutoSuggestInput from './AutoSuggestInput';

export interface AutoCompleteInputProps {
  name: string;
  readOnly?: boolean;
  value?: string;
  values: string[];
  onChange: (change: InputChanged<string>) => unknown;
}

function AutoCompleteInput({
  name,
  value = '',
  values,
  onChange,
  ...otherProps
}: AutoCompleteInputProps) {
  const [suggestions, setSuggestions] = useState<string[]>([]);

  const getSuggestionValue = useCallback((item: string) => {
    return item;
  }, []);

  const renderSuggestion = useCallback((item: string) => {
    return item;
  }, []);

  const handleInputChange = useCallback(
    (_event: SyntheticEvent, { newValue }: ChangeEvent) => {
      onChange({
        name,
        value: newValue,
      });
    },
    [name, onChange]
  );

  const handleInputBlur = useCallback(() => {
    setSuggestions([]);
  }, [setSuggestions]);

  const handleSuggestionsFetchRequested = useCallback(
    ({ value: newValue }: SuggestionsFetchRequestedParams) => {
      const lowerCaseValue = jdu.replace(newValue).toLowerCase();

      const filteredValues = values.filter((v) => {
        return jdu.replace(v).toLowerCase().includes(lowerCaseValue);
      });

      setSuggestions(filteredValues);
    },
    [values, setSuggestions]
  );

  const handleSuggestionsClearRequested = useCallback(() => {
    setSuggestions([]);
  }, [setSuggestions]);

  return (
    <AutoSuggestInput
      {...otherProps}
      name={name}
      value={value}
      suggestions={suggestions}
      getSuggestionValue={getSuggestionValue}
      renderSuggestion={renderSuggestion}
      onInputChange={handleInputChange}
      onInputBlur={handleInputBlur}
      onSuggestionsFetchRequested={handleSuggestionsFetchRequested}
      onSuggestionsClearRequested={handleSuggestionsClearRequested}
    />
  );
}

export default AutoCompleteInput;
