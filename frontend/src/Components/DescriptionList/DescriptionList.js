import PropTypes from 'prop-types';
import React, { Component } from 'react';
import styles from './DescriptionList.css';

class DescriptionList extends Component {

  //
  // Render

  render() {
    const {
      className,
      children
    } = this.props;

    return (
      <dl className={className}>
        {children}
      </dl>
    );
  }
}

DescriptionList.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.node
};

DescriptionList.defaultProps = {
  className: styles.descriptionList
};

export default DescriptionList;
