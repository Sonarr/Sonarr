import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { scrollDirections } from 'Helpers/Props';
import OverlayScroller from 'Components/Scroller/OverlayScroller';
import Scroller from 'Components/Scroller/Scroller';
import styles from './PageContentBody.css';

class PageContentBody extends Component {

  //
  // Render

  render() {
    const {
      className,
      innerClassName,
      isSmallScreen,
      children,
      dispatch,
      ...otherProps
    } = this.props;

    const ScrollerComponent = isSmallScreen ? Scroller : OverlayScroller;

    return (
      <ScrollerComponent
        className={className}
        scrollDirection={scrollDirections.VERTICAL}
        {...otherProps}
      >
        <div className={innerClassName}>
          {children}
        </div>
      </ScrollerComponent>
    );
  }
}

PageContentBody.propTypes = {
  className: PropTypes.string,
  innerClassName: PropTypes.string,
  isSmallScreen: PropTypes.bool.isRequired,
  children: PropTypes.node.isRequired,
  dispatch: PropTypes.func
};

PageContentBody.defaultProps = {
  className: styles.contentBody,
  innerClassName: styles.innerContentBody
};

export default PageContentBody;
