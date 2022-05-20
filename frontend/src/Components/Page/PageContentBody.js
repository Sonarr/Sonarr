import PropTypes from 'prop-types';
import React, { Component } from 'react';
import OverlayScroller from 'Components/Scroller/OverlayScroller';
import Scroller from 'Components/Scroller/Scroller';
import { scrollDirections } from 'Helpers/Props';
import { isFirefox, isMobile } from 'Utilities/browser';
import { isLocked } from 'Utilities/scrollLock';
import styles from './PageContentBody.css';

class PageContentBody extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._isMobile = isMobile();
    this._isSmallScreenFirefox = isFirefox && window.innerWidth < 768;
  }

  //
  // Listeners

  onScroll = (props) => {
    const { onScroll } = this.props;

    if (this.props.onScroll && !isLocked()) {
      onScroll(props);
    }
  };

  //
  // Render

  render() {
    const {
      className,
      innerClassName,
      children,
      dispatch,
      ...otherProps
    } = this.props;

    const ScrollerComponent = this._isMobile || this._isSmallScreenFirefox ?
      Scroller :
      OverlayScroller;

    return (
      <ScrollerComponent
        className={className}
        scrollDirection={scrollDirections.VERTICAL}
        {...otherProps}
        onScroll={this.onScroll}
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
  children: PropTypes.node.isRequired,
  onScroll: PropTypes.func,
  dispatch: PropTypes.func
};

PageContentBody.defaultProps = {
  className: styles.contentBody,
  innerClassName: styles.innerContentBody
};

export default PageContentBody;
