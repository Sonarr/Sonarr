import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Link from './Link';
import styles from './IconButton.css';

function IconButton(props) {
  const {
    className,
    iconClassName,
    name,
    kind,
    size,
    isSpinning,
    isDisabled,
    ...otherProps
  } = props;

  return (
    <Link
      className={classNames(
        className,
        isDisabled && styles.isDisabled
      )}
      aria-label="Table Options Button"
      isDisabled={isDisabled}
      {...otherProps}
    >
      <Icon
        className={iconClassName}
        name={name}
        kind={kind}
        size={size}
        isSpinning={isSpinning}
      />
    </Link>
  );
}

IconButton.propTypes = {
  ...Link.propTypes,
  className: PropTypes.string.isRequired,
  iconClassName: PropTypes.string,
  kind: PropTypes.string,
  name: PropTypes.object.isRequired,
  size: PropTypes.number,
  title: PropTypes.string,
  isSpinning: PropTypes.bool,
  isDisabled: PropTypes.bool
};

IconButton.defaultProps = {
  className: styles.button,
  size: 12
};

export default IconButton;
