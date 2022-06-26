import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './FormInputHelpText.css';

function FormInputHelpText(props) {
  const {
    className,
    text,
    link,
    tooltip,
    isError,
    isWarning,
    isCheckInput
  } = props;

  return (
    <div className={classNames(
      className,
      isError && styles.isError,
      isWarning && styles.isWarning,
      isCheckInput && styles.isCheckInput
    )}
    >
      {text}

      {
        link ?
          <Link
            className={styles.link}
            to={link}
            title={tooltip}
          >
            <Icon
              name={icons.EXTERNAL_LINK}
            />
          </Link> :
          null
      }

      {
        !link && tooltip ?
          <Icon
            containerClassName={styles.details}
            name={icons.INFO}
            title={tooltip}
          /> :
          null
      }
    </div>
  );
}

FormInputHelpText.propTypes = {
  className: PropTypes.string.isRequired,
  text: PropTypes.string.isRequired,
  link: PropTypes.string,
  tooltip: PropTypes.string,
  isError: PropTypes.bool,
  isWarning: PropTypes.bool,
  isCheckInput: PropTypes.bool
};

FormInputHelpText.defaultProps = {
  className: styles.helpText,
  isError: false,
  isWarning: false,
  isCheckInput: false
};

export default FormInputHelpText;
