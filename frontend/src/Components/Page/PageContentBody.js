import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { isLocked } from 'Utilities/scrollLock';
import { scrollDirections } from 'Helpers/Props';
import OverlayScroller from 'Components/Scroller/OverlayScroller';
import Scroller from 'Components/Scroller/Scroller';
import styles from './PageContentBody.css';

class PageContentBody extends Component {

  //
  // Listeners

  onScroll = (props) => {
    const { onScroll } = this.props;

    if (this.props.onScroll && !isLocked()) {
      onScroll(props);
    }
  }

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
  isSmallScreen: PropTypes.bool.isRequired,
  children: PropTypes.node.isRequired,
  onScroll: PropTypes.func,
  dispatch: PropTypes.func
};

PageContentBody.defaultProps = {
  className: styles.contentBody,
  innerClassName: styles.innerContentBody
};

export default PageContentBody;
