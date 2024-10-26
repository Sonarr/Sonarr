import classNames from 'classnames';
import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './FormInputHelpText.css';

interface FormInputHelpTextProps {
  className?: string;
  text: string;
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
  return (
    <div
      className={classNames(
        className,
        isError && styles.isError,
        isWarning && styles.isWarning,
        isCheckInput && styles.isCheckInput
      )}
    >
      {text}

      {link ? (
        <Link className={styles.link} to={link} title={tooltip}>
          <Icon name={icons.EXTERNAL_LINK} />
        </Link>
      ) : null}

      {!link && tooltip ? (
        <Icon
          containerClassName={styles.details}
          name={icons.INFO}
          title={tooltip}
        />
      ) : null}
    </div>
  );
}

export default FormInputHelpText;
