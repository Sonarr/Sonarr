import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Scroller from 'Components/Scroller/Scroller';
import styles from './MenuContent.css';

class MenuContent extends Component {

  //
  // Render

  render() {
    const {
      className,
      children,
      maxHeight
    } = this.props;

    return (
      <div
        className={className}
        style={{
          maxHeight: maxHeight ? `${maxHeight}px` : undefined
        }}
      >
        <Scroller className={styles.scroller}>
          {children}
        </Scroller>
      </div>
    );
  }
}

MenuContent.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired,
  maxHeight: PropTypes.number
};

MenuContent.defaultProps = {
  className: styles.menuContent
};

export default MenuContent;
