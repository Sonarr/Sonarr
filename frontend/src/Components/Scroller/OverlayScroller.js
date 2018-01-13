import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Scrollbars } from 'react-custom-scrollbars';
import { scrollDirections } from 'Helpers/Props';
import styles from './OverlayScroller.css';

class OverlayScroller extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._scroller = null;
  }

  componentDidUpdate(prevProps) {
    const {
      scrollTop
    } = this.props;

    if (scrollTop != null && scrollTop !== prevProps.scrollTop) {
      this._scroller.scrollTop(scrollTop);
    }
  }

  //
  // Control

  _setScrollRef = (ref) => {
    this._scroller = ref;
  }

  _renderThumb = (props) => {
    return (
      <div
        className={this.props.trackClassName}
        {...props}
      />
    );
  }

  _renderView = (props) => {
    return (
      <div
        className={this.props.className}
        {...props}
      />
    );
  }

  //
  // Listers

  onScroll = (event) => {
    const {
      scrollTop,
      scrollLeft
    } = event.currentTarget;

    const onScroll = this.props.onScroll;

    if (onScroll) {
      onScroll({ scrollTop, scrollLeft });
    }
  }

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
        renderThumbHorizontal={this._renderThumb}
        renderThumbVertical={this._renderThumb}
        renderView={this._renderView}
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
  onScroll: PropTypes.func
};

OverlayScroller.defaultProps = {
  className: styles.scroller,
  trackClassName: styles.thumb,
  scrollDirection: scrollDirections.VERTICAL,
  autoHide: false,
  autoScroll: true
};

export default OverlayScroller;
