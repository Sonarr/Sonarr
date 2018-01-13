import PropTypes from 'prop-types';
import React, { Component } from 'react';
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
      <div
        className={styles.modalHeader}
        {...otherProps}
      >
        {children}
      </div>
    );
  }

}

ModalHeader.propTypes = {
  children: PropTypes.node
};

export default ModalHeader;
