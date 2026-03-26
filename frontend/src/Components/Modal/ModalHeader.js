import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { ModalContext } from './Modal';
import styles from './ModalHeader.css';

class ModalHeader extends Component {

  //
  // Render

  render() {
    const {
      children,
      ...otherProps
    } = this.props;

    return (
      <ModalContext.Consumer>
        {({ headerId }) => (
          <div
            id={headerId}
            className={styles.modalHeader}
            {...otherProps}
          >
            {children}
          </div>
        )}
      </ModalContext.Consumer>
    );
  }

}

ModalHeader.propTypes = {
  children: PropTypes.node
};

export default ModalHeader;
