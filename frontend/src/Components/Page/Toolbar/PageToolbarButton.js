import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './PageToolbarButton.css';

function PageToolbarButton(props) {
  const {
    label,
    iconName,
    spinningName,
    isDisabled,
    isSpinning,
    ...otherProps
  } = props;

  return (
    <Link
      className={classNames(
        styles.toolbarButton,
        isDisabled && styles.isDisabled
      )}
      isDisabled={isDisabled || isSpinning}
      {...otherProps}
    >
      <Icon
        name={isSpinning ? (spinningName || iconName) : iconName}
        isSpinning={isSpinning}
        size={21}
      />

      <div className={styles.labelContainer}>
        <div className={styles.label}>
          {label}
        </div>
      </div>
    </Link>
  );
}

PageToolbarButton.propTypes = {
  label: PropTypes.string.isRequired,
  iconName: PropTypes.object.isRequired,
  spinningName: PropTypes.object,
  isSpinning: PropTypes.bool,
  isDisabled: PropTypes.bool
};

PageToolbarButton.defaultProps = {
  spinningName: icons.SPINNER,
  isDisabled: false,
  isSpinning: false
};

export default PageToolbarButton;
