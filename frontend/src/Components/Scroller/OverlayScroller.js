import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Scrollbars } from 'react-custom-scrollbars-2';
import { scrollDirections } from 'Helpers/Props';
import styles from './OverlayScroller.css';

const SCROLLBAR_SIZE = 10;

class OverlayScroller extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._scroller = null;
    this._isScrolling = false;
  }

  componentDidUpdate(prevProps) {
    const {
      scrollTop
    } = this.props;

    if (
      !this._isScrolling &&
      scrollTop != null &&
      scrollTop !== prevProps.scrollTop
    ) {
      this._scroller.scrollTop(scrollTop);
    }
  }

  //
  // Control

  _setScrollRef = (ref) => {
    this._scroller = ref;

    if (ref) {
      this.props.registerScroller(ref.view);
    }
  };

  _renderThumb = (props) => {
    return (
      <div
        className={this.props.trackClassName}
        {...props}
      />
    );
  };

  _renderTrackHorizontal = ({ style, props }) => {
    const finalStyle = {
      ...style,
      right: 2,
      bottom: 2,
      left: 2,
      borderRadius: 3,
      height: SCROLLBAR_SIZE
    };

    return (
      <div
        className={styles.track}
        style={finalStyle}
        {...props}
      />
    );
  };

  _renderTrackVertical = ({ style, props }) => {
    const finalStyle = {
      ...style,
      right: 2,
      bottom: 2,
      top: 2,
      borderRadius: 3,
      width: SCROLLBAR_SIZE
    };

    return (
      <div
        className={styles.track}
        style={finalStyle}
        {...props}
      />
    );
  };

  _renderView = (props) => {
    return (
      <div
        className={this.props.className}
        {...props}
      />
    );
  };

  //
  // Listers

  onScrollStart = () => {
    this._isScrolling = true;
  };

  onScrollStop = () => {
    this._isScrolling = false;
  };

  onScroll = (event) => {
    const {
      scrollTop,
      scrollLeft
    } = event.currentTarget;

    this._isScrolling = true;
    const onScroll = this.props.onScroll;

    if (onScroll) {
      onScroll({ scrollTop, scrollLeft });
    }
  };

  //
  // Render

  render() {
    const {
      autoHide,
      autoScroll,
      children
    } = this.props;

    return (
      <Scrollbars
        ref={this._setScrollRef}
        autoHide={autoHide}
        hideTracksWhenNotNeeded={autoScroll}
        renderTrackHorizontal={this._renderTrackHorizontal}
        renderTrackVertical={this._renderTrackVertical}
        renderThumbHorizontal={this._renderThumb}
        renderThumbVertical={this._renderThumb}
        renderView={this._renderView}
        onScrollStart={this.onScrollStart}
        onScrollStop={this.onScrollStop}
        onScroll={this.onScroll}
      >
        {children}
      </Scrollbars>
    );
  }

}

OverlayScroller.propTypes = {
  className: PropTypes.string,
  trackClassName: PropTypes.string,
  scrollTop: PropTypes.number,
  scrollDirection: PropTypes.oneOf([scrollDirections.NONE, scrollDirections.HORIZONTAL, scrollDirections.VERTICAL]).isRequired,
  autoHide: PropTypes.bool.isRequired,
  autoScroll: PropTypes.bool.isRequired,
  children: PropTypes.node,
  onScroll: PropTypes.func,
  registerScroller: PropTypes.func
};

OverlayScroller.defaultProps = {
  className: styles.scroller,
  trackClassName: styles.thumb,
  scrollDirection: scrollDirections.VERTICAL,
  autoHide: false,
  autoScroll: true,
  registerScroller: () => { /* no-op */ }
};

export default OverlayScroller;
