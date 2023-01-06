import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import { sizes } from 'Helpers/Props';
import styles from './FormLabel.css';

function FormLabel(props) {
  const {
    children,
    className,
    errorClassName,
    size,
    name,
    hasError,
    isAdvanced,
    ...otherProps
  } = props;

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
  children: PropTypes.oneOfType([PropTypes.node, PropTypes.string]).isRequired,
  className: PropTypes.string,
  errorClassName: PropTypes.string,
  size: PropTypes.oneOf(sizes.all),
  name: PropTypes.string,
  hasError: PropTypes.bool,
  isAdvanced: PropTypes.bool
};

FormLabel.defaultProps = {
  className: styles.label,
  errorClassName: styles.hasError,
  isAdvanced: false,
  size: sizes.LARGE
};

export default FormLabel;
