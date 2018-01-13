import PropTypes from 'prop-types';
import React, { Component } from 'react';
import styles from './PageContentFooter.css';

class PageContentFooter extends Component {

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

PageContentFooter.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired
};

PageContentFooter.defaultProps = {
  className: styles.contentFooter
};

export default PageContentFooter;
