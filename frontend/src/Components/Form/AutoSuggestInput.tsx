import classNames from 'classnames';
import React, {
  FocusEvent,
  FormEvent,
  KeyboardEvent,
  KeyboardEventHandler,
  MutableRefObject,
  ReactNode,
  Ref,
  SyntheticEvent,
  useCallback,
  useEffect,
  useRef,
} from 'react';
import Autosuggest, {
  AutosuggestPropsBase,
  BlurEvent,
  ChangeEvent,
  RenderInputComponentProps,
  RenderSuggestionsContainerParams,
} from 'react-autosuggest';
import { Manager, Popper, Reference } from 'react-popper';
import Portal from 'Components/Portal';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { InputChanged } from 'typings/inputs';
import styles from './AutoSuggestInput.css';

interface AutoSuggestInputProps<T>
  extends Omit<AutosuggestPropsBase<T>, 'renderInputComponent' | 'inputProps'> {
  forwardedRef?: MutableRefObject<Autosuggest<T> | null>;
  className?: string;
  inputContainerClassName?: string;
  name: string;
  value?: string;
  placeholder?: string;
  suggestions: T[];
  hasError?: boolean;
  hasWarning?: boolean;
  enforceMaxHeight?: boolean;
  minHeight?: number;
  maxHeight?: number;
  renderInputComponent?: (
    inputProps: RenderInputComponentProps,
    ref: Ref<HTMLDivElement>
  ) => ReactNode;
  onInputChange: (
    event: FormEvent<HTMLElement>,
    params: ChangeEvent
  ) => unknown;
  onInputKeyDown?: KeyboardEventHandler<HTMLElement>;
  onInputFocus?: (event: SyntheticEvent) => unknown;
  onInputBlur: (
    event: FocusEvent<HTMLElement>,
    params?: BlurEvent<T>
  ) => unknown;
  onChange?: (change: InputChanged<T>) => unknown;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function AutoSuggestInput<T = any>(props: AutoSuggestInputProps<T>) {
  const {
    // TODO: forwaredRef should be replaces with React.forwardRef
    forwardedRef,
    className = styles.input,
    inputContainerClassName = styles.inputContainer,
    name,
    value = '',
    placeholder,
    suggestions,
    enforceMaxHeight = true,
    hasError,
    hasWarning,
    minHeight = 50,
    maxHeight = 200,
    getSuggestionValue,
    renderSuggestion,
    renderInputComponent,
    onInputChange,
    onInputKeyDown,
    onInputFocus,
    onInputBlur,
    onSuggestionsFetchRequested,
    onSuggestionsClearRequested,
    onSuggestionSelected,
    onChange,
    ...otherProps
  } = props;

  const updater = useRef<(() => void) | null>(null);
  const previousSuggestions = usePrevious(suggestions);

  const handleComputeMaxHeight = useCallback(
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (data: any) => {
      const { top, bottom, width } = data.offsets.reference;

      if (enforceMaxHeight) {
        data.styles.maxHeight = maxHeight;
      } else {
        const windowHeight = window.innerHeight;

        if (/^botton/.test(data.placement)) {
          data.styles.maxHeight = windowHeight - bottom;
        } else {
          data.styles.maxHeight = top;
        }
      }

      data.styles.width = width;

      return data;
    },
    [enforceMaxHeight, maxHeight]
  );

  const createRenderInputComponent = useCallback(
    (inputProps: RenderInputComponentProps) => {
      return (
        <Reference>
          {({ ref }) => {
            if (renderInputComponent) {
              return renderInputComponent(inputProps, ref);
            }

            return (
              <div ref={ref}>
                <input {...inputProps} />
              </div>
            );
          }}
        </Reference>
      );
    },
    [renderInputComponent]
  );

  const renderSuggestionsContainer = useCallback(
    ({ containerProps, children }: RenderSuggestionsContainerParams) => {
      return (
        <Portal>
          <Popper
            placement="bottom-start"
            modifiers={{
              computeMaxHeight: {
                order: 851,
                enabled: true,
                fn: handleComputeMaxHeight,
              },
              flip: {
                padding: minHeight,
              },
            }}
          >
            {({ ref: popperRef, style, scheduleUpdate }) => {
              updater.current = scheduleUpdate;

              return (
                <div
                  ref={popperRef}
                  style={style}
                  className={
                    children ? styles.suggestionsContainerOpen : undefined
                  }
                >
                  <div
                    {...containerProps}
                    style={{
                      maxHeight: style.maxHeight,
                    }}
                  >
                    {children}
                  </div>
                </div>
              );
            }}
          </Popper>
        </Portal>
      );
    },
    [minHeight, handleComputeMaxHeight]
  );

  const handleInputKeyDown = useCallback(
    (event: KeyboardEvent<HTMLElement>) => {
      if (
        event.key === 'Tab' &&
        suggestions.length &&
        suggestions[0] !== value
      ) {
        event.preventDefault();

        if (value) {
          onSuggestionSelected?.(event, {
            suggestion: suggestions[0],
            suggestionValue: value,
            suggestionIndex: 0,
            sectionIndex: null,
            method: 'enter',
          });
        }
      }
    },
    [value, suggestions, onSuggestionSelected]
  );

  const inputProps = {
    className: classNames(
      className,
      hasError && styles.hasError,
      hasWarning && styles.hasWarning
    ),
    name,
    value,
    placeholder,
    autoComplete: 'off',
    spellCheck: false,
    onChange: onInputChange,
    onKeyDown: onInputKeyDown || handleInputKeyDown,
    onFocus: onInputFocus,
    onBlur: onInputBlur,
  };

  const theme = {
    container: inputContainerClassName,
    containerOpen: styles.suggestionsContainerOpen,
    suggestionsContainer: styles.suggestionsContainer,
    suggestionsList: styles.suggestionsList,
    suggestion: styles.suggestion,
    suggestionHighlighted: styles.suggestionHighlighted,
  };

  useEffect(() => {
    if (updater.current && suggestions !== previousSuggestions) {
      updater.current();
    }
  }, [suggestions, previousSuggestions]);

  return (
    <Manager>
      <Autosuggest
        {...otherProps}
        ref={forwardedRef}
        id={name}
        inputProps={inputProps}
        theme={theme}
        suggestions={suggestions}
        getSuggestionValue={getSuggestionValue}
        renderInputComponent={createRenderInputComponent}
        renderSuggestionsContainer={renderSuggestionsContainer}
        renderSuggestion={renderSuggestion}
        onSuggestionSelected={onSuggestionSelected}
        onSuggestionsFetchRequested={onSuggestionsFetchRequested}
        onSuggestionsClearRequested={onSuggestionsClearRequested}
      />
    </Manager>
  );
}

export default AutoSuggestInput;
