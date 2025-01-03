import classNames from 'classnames';
import React, {
  ChangeEvent,
  FocusEvent,
  SyntheticEvent,
  useCallback,
  useEffect,
  useRef,
} from 'react';
import { FileInputChanged, InputChanged } from 'typings/inputs';
import styles from './TextInput.css';

export interface CommonTextInputProps {
  className?: string;
  readOnly?: boolean;
  autoFocus?: boolean;
  placeholder?: string;
  name: string;
  value: string | number | string[];
  hasError?: boolean;
  hasWarning?: boolean;
  hasButton?: boolean;
  step?: number;
  min?: number;
  max?: number;
  onFocus?: (event: FocusEvent<HTMLInputElement, Element>) => void;
  onBlur?: (event: SyntheticEvent) => void;
  onCopy?: (event: SyntheticEvent) => void;
  onSelectionChange?: (start: number | null, end: number | null) => void;
}

export interface TextInputProps extends CommonTextInputProps {
  type?: 'date' | 'number' | 'password' | 'text';
  onChange: (change: InputChanged<string>) => void;
}

export interface FileInputProps extends CommonTextInputProps {
  type: 'file';
  onChange: (change: FileInputChanged) => void;
}

function TextInput({
  className = styles.input,
  type = 'text',
  readOnly = false,
  autoFocus = false,
  placeholder,
  name,
  value = '',
  hasError,
  hasWarning,
  hasButton,
  step,
  min,
  max,
  onBlur,
  onFocus,
  onCopy,
  onChange,
  onSelectionChange,
}: TextInputProps | FileInputProps): JSX.Element {
  const inputRef = useRef<HTMLInputElement>(null);
  const selectionTimeout = useRef<ReturnType<typeof setTimeout>>();
  const selectionStart = useRef<number | null>();
  const selectionEnd = useRef<number | null>();
  const isMouseTarget = useRef(false);

  const selectionChanged = useCallback(() => {
    if (selectionTimeout.current) {
      clearTimeout(selectionTimeout.current);
    }

    selectionTimeout.current = setTimeout(() => {
      if (!inputRef.current) {
        return;
      }

      const start = inputRef.current.selectionStart;
      const end = inputRef.current.selectionEnd;

      const selectionChanged =
        selectionStart.current !== start || selectionEnd.current !== end;

      selectionStart.current = start;
      selectionEnd.current = end;

      if (selectionChanged) {
        onSelectionChange?.(start, end);
      }
    }, 10);
  }, [onSelectionChange]);

  const handleChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      onChange({
        name,
        value: event.target.value,
        files: type === 'file' ? event.target.files : undefined,
      });
    },
    [name, type, onChange]
  );

  const handleFocus = useCallback(
    (event: FocusEvent<HTMLInputElement, Element>) => {
      onFocus?.(event);

      selectionChanged();
    },
    [selectionChanged, onFocus]
  );

  const handleKeyUp = useCallback(() => {
    selectionChanged();
  }, [selectionChanged]);

  const handleMouseDown = useCallback(() => {
    isMouseTarget.current = true;
  }, []);

  const handleMouseUp = useCallback(() => {
    selectionChanged();
  }, [selectionChanged]);

  const handleWheel = useCallback(() => {
    if (type === 'number') {
      inputRef.current?.blur();
    }
  }, [type]);

  const handleDocumentMouseUp = useCallback(() => {
    if (isMouseTarget.current) {
      selectionChanged();
    }

    isMouseTarget.current = false;
  }, [selectionChanged]);

  useEffect(() => {
    window.addEventListener('mouseup', handleDocumentMouseUp);

    return () => {
      window.removeEventListener('mouseup', handleDocumentMouseUp);
    };
  }, [handleDocumentMouseUp]);

  useEffect(() => {
    return () => {
      clearTimeout(selectionTimeout.current);
    };
  }, []);

  return (
    <input
      ref={inputRef}
      type={type}
      readOnly={readOnly}
      autoFocus={autoFocus}
      placeholder={placeholder}
      className={classNames(
        className,
        readOnly && styles.readOnly,
        hasError && styles.hasError,
        hasWarning && styles.hasWarning,
        hasButton && styles.hasButton
      )}
      name={name}
      value={value}
      step={step}
      min={min}
      max={max}
      onChange={handleChange}
      onFocus={handleFocus}
      onBlur={onBlur}
      onCopy={onCopy}
      onCut={onCopy}
      onKeyUp={handleKeyUp}
      onMouseDown={handleMouseDown}
      onMouseUp={handleMouseUp}
      onWheel={handleWheel}
    />
  );
}

export default TextInput;
