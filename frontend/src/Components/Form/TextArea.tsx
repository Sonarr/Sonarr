import classNames from 'classnames';
import React, {
  ChangeEvent,
  SyntheticEvent,
  useCallback,
  useEffect,
  useRef,
} from 'react';
import { InputChanged } from 'typings/inputs';
import styles from './TextArea.css';

export interface TextAreaProps {
  className?: string;
  readOnly?: boolean;
  autoFocus?: boolean;
  placeholder?: string;
  name: string;
  value?: string;
  hasError?: boolean;
  hasWarning?: boolean;
  onChange: (change: InputChanged<string>) => void;
  onFocus?: (event: SyntheticEvent) => void;
  onBlur?: (event: SyntheticEvent) => void;
  onSelectionChange?: (start: number | null, end: number | null) => void;
}

function TextArea({
  className = styles.input,
  readOnly = false,
  autoFocus = false,
  placeholder,
  name,
  value = '',
  hasError,
  hasWarning,
  onBlur,
  onFocus,
  onChange,
  onSelectionChange,
}: TextAreaProps) {
  const inputRef = useRef<HTMLTextAreaElement>(null);
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
    (event: ChangeEvent<HTMLTextAreaElement>) => {
      onChange({
        name,
        value: event.target.value,
      });
    },
    [name, onChange]
  );

  const handleFocus = useCallback(
    (event: SyntheticEvent) => {
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

  return (
    <textarea
      ref={inputRef}
      readOnly={readOnly}
      autoFocus={autoFocus}
      placeholder={placeholder}
      className={classNames(
        className,
        readOnly && styles.readOnly,
        hasError && styles.hasError,
        hasWarning && styles.hasWarning
      )}
      name={name}
      value={value}
      onChange={handleChange}
      onFocus={handleFocus}
      onBlur={onBlur}
      onKeyUp={handleKeyUp}
      onMouseDown={handleMouseDown}
      onMouseUp={handleMouseUp}
    />
  );
}

export default TextArea;
