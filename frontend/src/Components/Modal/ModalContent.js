import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Link from 'Components/Link/Link';
import Icon from 'Components/Icon';
import styles from './ModalContent.css';

function ModalContent(props) {
  const {
    className,
    children,
    onModalClose,
    ...otherProps
  } = props;

  return (
    <div
      className={className}
      {...otherProps}
    >
      <Link
        className={styles.closeButton}
        onPress={onModalClose}
      >
        <Icon
          name={icons.CLOSE}
          size={18}
        />
      </Link>

      {children}
    </div>
  );
}

ModalContent.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node,
  onModalClose: PropTypes.func.isRequired
};

ModalContent.defaultProps = {
  className: styles.modalContent
};

export default ModalContent;
