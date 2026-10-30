import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { useFormRow } from './FormRowContext';
import styles from './FormLabel.module.css';

interface FormLabelProps {
  children: ReactNode;
  className?: string;
  errorClassName?: string;
  name?: string;
  hasError?: boolean;
  isAdvanced?: boolean;
}

function FormLabel(props: FormLabelProps) {
  const formRow = useFormRow();
  const {
    children,
    className = styles.label,
    errorClassName = styles.hasError,
    name,
    hasError,
    isAdvanced = formRow?.isAdvanced ?? false,
  } = props;

  return (
    <label
      className={classNames(
        className,
        hasError && errorClassName,
        isAdvanced && styles.isAdvanced
      )}
      htmlFor={formRow?.id ?? name}
    >
      {children}
    </label>
  );
}

export default FormLabel;
