import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './ModalContent.css';

function ModalContent(props) {
  const {
    className,
    children,
    showCloseButton,
    onModalClose,
    ...otherProps
  } = props;

  return (
    <div
      className={className}
      {...otherProps}
    >
      {
        showCloseButton &&
          <Link
            className={styles.closeButton}
            onPress={onModalClose}
          >
            <Icon
              name={icons.CLOSE}
              size={18}
            />
          </Link>
      }

      {children}
    </div>
  );
}

ModalContent.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node,
  showCloseButton: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

ModalContent.defaultProps = {
  className: styles.modalContent,
  showCloseButton: true
};

export default ModalContent;
