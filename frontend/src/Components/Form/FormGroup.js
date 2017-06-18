import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import { map } from 'Helpers/elementChildren';
import { sizes } from 'Helpers/Props';
import styles from './FormGroup.css';

function FormGroup(props) {
  const {
    className,
    children,
    size,
    advancedSettings,
    isAdvanced,
    ...otherProps
  } = props;

  if (!advancedSettings && isAdvanced) {
    return null;
  }

  const childProps = isAdvanced ? { isAdvanced } : {};

  return (
    <div
      className={classNames(
        className,
        styles[size]
      )}
      {...otherProps}
    >
      {
        map(children, (child) => {
          return React.cloneElement(child, childProps);
        })
      }
    </div>
  );
}

FormGroup.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.node.isRequired,
  size: PropTypes.oneOf(sizes.all).isRequired,
  advancedSettings: PropTypes.bool.isRequired,
  isAdvanced: PropTypes.bool.isRequired
};

FormGroup.defaultProps = {
  className: styles.group,
  size: sizes.SMALL,
  advancedSettings: false,
  isAdvanced: false
};

export default FormGroup;
