import classNames from 'classnames';
import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import { useFormRow } from './FormRowContext';
import styles from './FormInputHelpText.module.css';

interface FormInputHelpTextProps {
  className?: string;
  text?: string;
  link?: string;
  tooltip?: string;
  isError?: boolean;
  isWarning?: boolean;
  isCheckInput?: boolean;
  name?: string;
}

function FormInputHelpText({
  className = styles.helpText,
  text,
  link,
  tooltip,
  isError = false,
  isWarning = false,
  isCheckInput = false,
  name,
}: FormInputHelpTextProps) {
  const formRow = useFormRow();

  if (!text) {
    return null;
  }

  const content = (
    <>
      {text}

      {link ? (
        <Link className={styles.link} to={link} title={tooltip}>
          <Icon name={icons.EXTERNAL_LINK} />
        </Link>
      ) : null}

      {!link && tooltip ? (
        <Icon
          titleWrapperClassName={styles.details}
          name={icons.INFO}
          title={tooltip}
        />
      ) : null}
    </>
  );

  const inputName = formRow?.id ?? name;
  const helpTextClassName = classNames(
    className,
    isError && styles.isError,
    isWarning && styles.isWarning,
    isCheckInput && styles.isCheckInput
  );

  return inputName && formRow?.type === 'check' ? (
    <label className={helpTextClassName} htmlFor={inputName}>
      {content}
    </label>
  ) : (
    <div className={helpTextClassName}>{content}</div>
  );
}

export default FormInputHelpText;
