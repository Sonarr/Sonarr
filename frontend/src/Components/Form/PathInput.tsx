import classNames from 'classnames';
import React, {
  KeyboardEvent,
  SyntheticEvent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import {
  ChangeEvent,
  SuggestionsFetchRequestedParams,
} from 'react-autosuggest';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons } from 'Helpers/Props';
import usePaths, { Path } from 'Path/usePaths';
import { InputChanged } from 'typings/inputs';
import AutoSuggestInput from './AutoSuggestInput';
import FormInputButton from './FormInputButton';
import styles from './PathInput.css';

export interface PathInputProps {
  className?: string;
  name: string;
  value?: string;
  placeholder?: string;
  includeFiles: boolean;
  hasButton?: boolean;
  hasFileBrowser?: boolean;
  onChange: (change: InputChanged<string>) => void;
}

interface PathInputInternalProps extends PathInputProps {
  paths: Path[];
  onFetchPaths: (path: string) => void;
  onClearPaths: () => void;
}

function handleSuggestionsClearRequested() {
  // Required because props aren't always rendered, but no-op
  // because we don't want to reset the paths after a path is selected.
}

function PathInput(props: PathInputProps) {
  const { includeFiles, value = '' } = props;
  const [currentPath, setCurrentPath] = useState(value);

  const { data } = usePaths({
    path: currentPath,
    includeFiles,
  });

  const handleFetchPaths = useCallback((path: string) => {
    setCurrentPath(path);
  }, []);

  const handleClearPaths = useCallback(() => {
    // No-op for React Query implementation as we don't need to clear
  }, []);

  return (
    <PathInputInternal
      {...props}
      paths={data.paths}
      onFetchPaths={handleFetchPaths}
      onClearPaths={handleClearPaths}
    />
  );
}

export default PathInput;

export function PathInputInternal({
  className = styles.inputWrapper,
  name,
  value: inputValue = '',
  paths,
  includeFiles,
  hasButton,
  hasFileBrowser = true,
  onChange,
  onFetchPaths,
  onClearPaths,
  ...otherProps
}: PathInputInternalProps) {
  const [value, setValue] = useState(inputValue);
  const [isFileBrowserModalOpen, setIsFileBrowserModalOpen] = useState(false);
  const previousInputValue = usePrevious(inputValue);

  const handleInputChange = useCallback(
    (_event: SyntheticEvent, { newValue }: ChangeEvent) => {
      setValue(newValue);
    },
    [setValue]
  );

  const handleInputKeyDown = useCallback(
    (event: KeyboardEvent<HTMLElement>) => {
      if (event.key === 'Tab') {
        event.preventDefault();
        const path = paths[0];

        if (path) {
          onChange({
            name,
            value: path.path,
          });

          if (path.type !== 'file') {
            onFetchPaths(path.path);
          }
        }
      }
    },
    [name, paths, onFetchPaths, onChange]
  );
  const handleInputBlur = useCallback(() => {
    onChange({
      name,
      value,
    });

    onClearPaths();
  }, [name, value, onClearPaths, onChange]);

  const handleSuggestionSelected = useCallback(
    (_event: SyntheticEvent, { suggestion }: { suggestion: Path }) => {
      onFetchPaths(suggestion.path);
    },
    [onFetchPaths]
  );

  const handleSuggestionsFetchRequested = useCallback(
    ({ value: newValue }: SuggestionsFetchRequestedParams) => {
      onFetchPaths(newValue);
    },
    [onFetchPaths]
  );

  const handleFileBrowserOpenPress = useCallback(() => {
    setIsFileBrowserModalOpen(true);
  }, [setIsFileBrowserModalOpen]);

  const handleFileBrowserModalClose = useCallback(() => {
    setIsFileBrowserModalOpen(false);
  }, [setIsFileBrowserModalOpen]);

  const handleChange = useCallback(
    (change: InputChanged<Path>) => {
      onChange({ name, value: change.value.path });
    },
    [name, onChange]
  );

  const getSuggestionValue = useCallback(({ path }: Path) => path, []);

  const renderSuggestion = useCallback(
    ({ path }: Path, { query }: { query: string }) => {
      const lastSeparatorIndex =
        query.lastIndexOf('\\') || query.lastIndexOf('/');

      if (lastSeparatorIndex === -1) {
        return <span>{path}</span>;
      }

      return (
        <span>
          <span className={styles.pathMatch}>
            {path.substring(0, lastSeparatorIndex)}
          </span>
          {path.substring(lastSeparatorIndex)}
        </span>
      );
    },
    []
  );

  useEffect(() => {
    if (inputValue !== previousInputValue) {
      setValue(inputValue);
    }
  }, [inputValue, previousInputValue, setValue]);

  return (
    <div className={className}>
      <AutoSuggestInput
        {...otherProps}
        className={hasFileBrowser ? styles.hasFileBrowser : undefined}
        name={name}
        value={value}
        suggestions={paths}
        getSuggestionValue={getSuggestionValue}
        renderSuggestion={renderSuggestion}
        onInputKeyDown={handleInputKeyDown}
        onInputChange={handleInputChange}
        onInputBlur={handleInputBlur}
        onSuggestionSelected={handleSuggestionSelected}
        onSuggestionsFetchRequested={handleSuggestionsFetchRequested}
        onSuggestionsClearRequested={handleSuggestionsClearRequested}
        onChange={handleChange}
      />

      {hasFileBrowser ? (
        <>
          <FormInputButton
            className={classNames(
              styles.fileBrowserButton,
              hasButton && styles.fileBrowserMiddleButton
            )}
            onPress={handleFileBrowserOpenPress}
          >
            <Icon name={icons.FOLDER_OPEN} />
          </FormInputButton>

          <FileBrowserModal
            isOpen={isFileBrowserModalOpen}
            name={name}
            value={value}
            includeFiles={includeFiles}
            onChange={onChange}
            onModalClose={handleFileBrowserModalClose}
          />
        </>
      ) : null}
    </div>
  );
}
