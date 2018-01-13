import PropTypes from 'prop-types';
import React, { Component } from 'react';
import styles from './ModalFooter.css';

class ModalFooter extends Component {

  //
  // Render

  render() {
    const {
      children,
      ...otherProps
    } = this.props;

    return (
      <div
        className={styles.modalFooter}
        {...otherProps}
      >
        {children}
      </div>
    );
  }

}

ModalFooter.propTypes = {
  children: PropTypes.node
};

export default ModalFooter;
