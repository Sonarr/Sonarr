import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import { sizes } from 'Helpers/Props';
import styles from './FormLabel.css';

function FormLabel({
  children,
  className,
  errorClassName,
  size,
  name,
  hasError,
  isAdvanced,
  ...otherProps
}) {
  return (
    <label
      {...otherProps}
      className={classNames(
        className,
        styles[size],
        hasError && errorClassName,
        isAdvanced && styles.isAdvanced
      )}
      htmlFor={name}
    >
      {children}
    </label>
  );
}

FormLabel.propTypes = {
  children: PropTypes.node.isRequired,
  className: PropTypes.string,
  errorClassName: PropTypes.string,
  size: PropTypes.oneOf(sizes.all),
  name: PropTypes.string,
  hasError: PropTypes.bool,
  isAdvanced: PropTypes.bool.isRequired
};

FormLabel.defaultProps = {
  className: styles.label,
  errorClassName: styles.hasError,
  isAdvanced: false,
  size: sizes.LARGE
};

export default FormLabel;
