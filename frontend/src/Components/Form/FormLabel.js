import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import styles from './FormLabel.css';

function FormLabel({
  children,
  className,
  errorClassName,
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
  name: PropTypes.string,
  hasError: PropTypes.bool,
  isAdvanced: PropTypes.bool.isRequired
};

FormLabel.defaultProps = {
  className: styles.label,
  errorClassName: styles.hasError,
  isAdvanced: false
};

export default FormLabel;
