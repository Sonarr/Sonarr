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
    ...otherProps
  } = props;

  return (
    <Link
      className={className}
      {...otherProps}
    >
      <Icon
        className={iconClassName}
        name={name}
        kind={kind}
        size={size}
      />
    </Link>
  );
}

IconButton.propTypes = {
  className: PropTypes.string.isRequired,
  iconClassName: PropTypes.string,
  kind: PropTypes.string,
  name: PropTypes.string.isRequired,
  size: PropTypes.number
};

IconButton.defaultProps = {
  className: styles.button
};

export default IconButton;
