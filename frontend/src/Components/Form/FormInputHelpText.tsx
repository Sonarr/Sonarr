import classNames from 'classnames';
import React, { useEffect, useId } from 'react';
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
}

function FormInputHelpText({
  className = styles.helpText,
  text,
  link,
  tooltip,
  isError = false,
  isWarning = false,
  isCheckInput = false,
}: FormInputHelpTextProps) {
  const formRow = useFormRow();
  const generatedId = useId();
  const inputId = formRow?.id;
  const formRowType = formRow?.type;
  const addDescriptionId = formRow?.addDescriptionId;
  const removeDescriptionId = formRow?.removeDescriptionId;
  const isCheckboxHelpText =
    inputId && formRowType === 'check' && !isError && !isWarning;

  useEffect(() => {
    if (
      !inputId ||
      isCheckboxHelpText ||
      !text ||
      !addDescriptionId ||
      !removeDescriptionId
    ) {
      return;
    }

    addDescriptionId(generatedId);

    return () => {
      removeDescriptionId(generatedId);
    };
  }, [
    addDescriptionId,
    generatedId,
    inputId,
    isCheckboxHelpText,
    removeDescriptionId,
    text,
  ]);

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

  const helpTextClassName = classNames(
    className,
    isError && styles.isError,
    isWarning && styles.isWarning,
    isCheckInput && styles.isCheckInput
  );

  return isCheckboxHelpText ? (
    <label className={helpTextClassName} htmlFor={inputId}>
      {content}
    </label>
  ) : (
    <div
      id={inputId ? generatedId : undefined}
      className={helpTextClassName}
      role={isError ? 'alert' : undefined}
    >
      {content}
    </div>
  );
}

export default FormInputHelpText;
