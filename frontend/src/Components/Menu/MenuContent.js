import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Scroller from 'Components/Scroller/Scroller';
import getUniqueElementId from 'Utilities/getUniqueElementId';
import styles from './MenuContent.css';

class MenuContent extends Component {

  //
  // Render

  render() {
    const {
      forwardedRef,
      className,
      id,
      children,
      style,
      isOpen
    } = this.props;

    return (
      <div
        id={id}
        ref={forwardedRef}
        className={className}
        style={style}
      >
        {
          isOpen ?
            <Scroller className={styles.scroller}>
              {children}
            </Scroller> :
            null
        }
      </div>
    );
  }
}

MenuContent.propTypes = {
  forwardedRef: PropTypes.func,
  className: PropTypes.string,
  id: PropTypes.string.isRequired,
  children: PropTypes.node.isRequired,
  style: PropTypes.object,
  isOpen: PropTypes.bool
};

MenuContent.defaultProps = {
  className: styles.menuContent,
  id: getUniqueElementId()
};

export default MenuContent;
