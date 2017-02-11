import PropTypes from 'prop-types';
import React, { Component } from 'react';
import styles from './FieldSet.css';

class FieldSet extends Component {

  //
  // Render

  render() {
    const {
      legend,
      children
    } = this.props;

    return (
      <fieldset className={styles.fieldSet}>
        <legend className={styles.legend}>
          {legend}
        </legend>
        {children}
      </fieldset>
    );
  }

}

FieldSet.propTypes = {
  legend: PropTypes.oneOfType([PropTypes.node, PropTypes.string]),
  children: PropTypes.node
};

export default FieldSet;
