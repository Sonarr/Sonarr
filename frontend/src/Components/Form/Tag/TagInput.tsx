import classNames from 'classnames';
import React, {
  KeyboardEvent,
  Ref,
  SyntheticEvent,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';
import {
  ChangeEvent,
  RenderInputComponentProps,
  RenderSuggestion,
  SuggestionsFetchRequestedParams,
} from 'react-autosuggest';
import useDebouncedCallback from 'Helpers/Hooks/useDebouncedCallback';
import { Kind } from 'Helpers/Props/kinds';
import { InputChanged } from 'typings/inputs';
import AutoSuggestInput from '../AutoSuggestInput';
import TagInputInput from './TagInputInput';
import TagInputTag, { EditedTag, TagInputTagProps } from './TagInputTag';
import styles from './TagInput.css';

export interface TagBase {
  id: boolean | number | string | null;
  name: string | number;
}

function getTag<T extends { id: T['id']; name: T['name'] }>(
  value: string,
  selectedIndex: number,
  suggestions: T[],
  allowNew: boolean
) {
  if (selectedIndex == null && value) {
    const existingTag = suggestions.find(
      (suggestion) => suggestion.name === value
    );

    if (existingTag) {
      return existingTag;
    } else if (allowNew) {
      return { name: value } as T;
    }
  } else if (selectedIndex != null) {
    return suggestions[selectedIndex];
  }

  return null;
}

function handleSuggestionsClearRequested() {
  // Required because props aren't always rendered, but no-op
  // because we don't want to reset the paths after a path is selected.
}

export interface ReplacementTag<T extends TagBase> {
  index: number;
  id: T['id'];
}

export interface TagInputProps<T extends TagBase> {
  className?: string;
  inputContainerClassName?: string;
  name: string;
  tags: T[];
  tagList: T[];
  allowNew?: boolean;
  kind?: Kind;
  placeholder?: string;
  delimiters?: string[];
  minQueryLength?: number;
  canEdit?: boolean;
  hasError?: boolean;
  hasWarning?: boolean;
  tagComponent?: React.ElementType;
  onChange?: (change: InputChanged<T['id'][]>) => void;
  onTagAdd: (newTag: T) => void;
  onTagDelete: TagInputTagProps<T>['onDelete'];
  onTagReplace?: (
    tagToReplace: ReplacementTag<T>,
    newTagName: T['name']
  ) => void;
}

function TagInput<T extends TagBase>({
  className = styles.internalInput,
  inputContainerClassName = styles.input,
  name,
  tags,
  tagList,
  allowNew = true,
  kind = 'info',
  placeholder = '',
  delimiters = ['Tab', 'Enter', ' ', ','],
  minQueryLength = 1,
  canEdit = false,
  tagComponent = TagInputTag,
  hasError,
  hasWarning,
  onChange,
  onTagAdd,
  onTagDelete,
  onTagReplace,
  ...otherProps
}: TagInputProps<T>) {
  const [value, setValue] = useState('');
  const [suggestions, setSuggestions] = useState<T[]>([]);
  const [isFocused, setIsFocused] = useState(false);
  const autoSuggestRef = useRef(null);

  const addTag = useDebouncedCallback(
    (tag: T | null) => {
      if (!tag) {
        return;
      }

      onTagAdd(tag);

      setValue('');
      setSuggestions([]);
    },
    250,
    {
      leading: true,
      trailing: false,
    }
  );

  const handleEditTag = useCallback(
    ({ value: newValue, ...otherProps }: EditedTag<T>) => {
      if (value && onTagReplace) {
        onTagReplace(otherProps, value);
      } else {
        onTagDelete(otherProps);
      }

      setValue(String(newValue));
    },
    [value, setValue, onTagDelete, onTagReplace]
  );

  const handleInputContainerPress = useCallback(() => {
    // @ts-expect-error Ref isn't typed yet
    autoSuggestRef?.current?.input.focus();
  }, []);

  const handleInputChange = useCallback(
    (_event: SyntheticEvent, { newValue, method }: ChangeEvent) => {
      const finalValue =
        // @ts-expect-error newValue may be an object?
        typeof newValue === 'object' ? newValue.name : newValue;

      if (method === 'type') {
        setValue(finalValue);
      }
    },
    [setValue]
  );

  const handleSuggestionsFetchRequested = useCallback(
    ({ value: newValue }: SuggestionsFetchRequestedParams) => {
      const lowerCaseValue = newValue.toLowerCase();

      const suggestions = tagList.filter((tag) => {
        return (
          String(tag.name).toLowerCase().includes(lowerCaseValue) &&
          !tags.some((t) => t.id === tag.id)
        );
      });

      setSuggestions(suggestions);
    },
    [tags, tagList, setSuggestions]
  );

  const handleInputKeyDown = useCallback(
    (event: KeyboardEvent<HTMLElement>) => {
      const key = event.key;

      if (!autoSuggestRef.current) {
        return;
      }

      if (key === 'Backspace' && !value.length) {
        const index = tags.length - 1;

        if (index >= 0) {
          onTagDelete({ index, id: tags[index].id });
        }

        setTimeout(() => {
          handleSuggestionsFetchRequested({
            value: '',
            reason: 'input-changed',
          });
        });

        event.preventDefault();
      }

      if (delimiters.includes(key)) {
        // @ts-expect-error Ref isn't typed yet
        const selectedIndex = autoSuggestRef.current.highlightedSuggestionIndex;
        const tag = getTag<T>(value, selectedIndex, suggestions, allowNew);

        if (tag) {
          addTag(tag);
          event.preventDefault();
        }
      }
    },
    [
      tags,
      allowNew,
      delimiters,
      onTagDelete,
      value,
      suggestions,
      addTag,
      handleSuggestionsFetchRequested,
    ]
  );

  const handleInputFocus = useCallback(() => {
    setIsFocused(true);
  }, [setIsFocused]);

  const handleInputBlur = useCallback(() => {
    setIsFocused(false);

    if (!autoSuggestRef.current) {
      return;
    }

    // @ts-expect-error Ref isn't typed yet
    const selectedIndex = autoSuggestRef.current.highlightedSuggestionIndex;
    const tag = getTag(value, selectedIndex, suggestions, allowNew);

    if (tag) {
      addTag(tag);
    }
  }, [allowNew, value, suggestions, autoSuggestRef, addTag, setIsFocused]);

  const handleSuggestionSelected = useCallback(
    (_event: SyntheticEvent, { suggestion }: { suggestion: T }) => {
      addTag(suggestion);
    },
    [addTag]
  );

  const getSuggestionValue = useCallback(({ name }: T): string => {
    return String(name);
  }, []);

  const shouldRenderSuggestions = useCallback(
    (v: string) => {
      return v.length >= minQueryLength;
    },
    [minQueryLength]
  );

  const renderSuggestion: RenderSuggestion<T> = useCallback(({ name }: T) => {
    return name;
  }, []);

  const renderInputComponent = useCallback(
    (
      inputProps: RenderInputComponentProps,
      forwardedRef: Ref<HTMLDivElement>
    ) => {
      return (
        <TagInputInput
          forwardedRef={forwardedRef}
          tags={tags}
          kind={kind}
          inputProps={inputProps}
          isFocused={isFocused}
          canEdit={canEdit}
          tagComponent={tagComponent}
          onTagDelete={onTagDelete}
          onTagEdit={handleEditTag}
          onInputContainerPress={handleInputContainerPress}
        />
      );
    },
    [
      tags,
      kind,
      canEdit,
      isFocused,
      tagComponent,
      handleInputContainerPress,
      handleEditTag,
      onTagDelete,
    ]
  );

  useEffect(() => {
    return () => {
      addTag.cancel();
    };
  }, [addTag]);

  return (
    <AutoSuggestInput
      {...otherProps}
      forwardedRef={autoSuggestRef}
      className={className}
      inputContainerClassName={classNames(
        inputContainerClassName,
        isFocused && styles.isFocused,
        hasError && styles.hasError,
        hasWarning && styles.hasWarning
      )}
      name={name}
      value={value}
      placeholder={placeholder}
      suggestions={suggestions}
      getSuggestionValue={getSuggestionValue}
      shouldRenderSuggestions={shouldRenderSuggestions}
      focusInputOnSuggestionClick={false}
      renderSuggestion={renderSuggestion}
      renderInputComponent={renderInputComponent}
      onInputChange={handleInputChange}
      onInputKeyDown={handleInputKeyDown}
      onInputFocus={handleInputFocus}
      onInputBlur={handleInputBlur}
      onSuggestionSelected={handleSuggestionSelected}
      onSuggestionsFetchRequested={handleSuggestionsFetchRequested}
      onSuggestionsClearRequested={handleSuggestionsClearRequested}
    />
  );
}

export default TagInput;
