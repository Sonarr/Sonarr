import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { sizes } from 'Helpers/Props';
import styles from './FieldSet.css';

class FieldSet extends Component {

  //
  // Render

  render() {
    const {
      size,
      legend,
      children
    } = this.props;

    return (
      <fieldset className={styles.fieldSet}>
        <legend className={classNames(styles.legend, (size === sizes.SMALL) && styles.small)}>
          {legend}
        </legend>
        {children}
      </fieldset>
    );
  }

}

FieldSet.propTypes = {
  size: PropTypes.oneOf(sizes.all).isRequired,
  legend: PropTypes.oneOfType([PropTypes.node, PropTypes.string]),
  children: PropTypes.node
};

FieldSet.defaultProps = {
  size: sizes.MEDIUM
};

export default FieldSet;
