import PropTypes from 'prop-types';
import React, { Component } from 'react';
import styles from './PageToolbar.css';

class PageToolbar extends Component {

  //
  // Render

  render() {
    const {
      className,
      children
    } = this.props;

    return (
      <div className={className}>
        {children}
      </div>
    );
  }
}

PageToolbar.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired
};

PageToolbar.defaultProps = {
  className: styles.toolbar
};

export default PageToolbar;
