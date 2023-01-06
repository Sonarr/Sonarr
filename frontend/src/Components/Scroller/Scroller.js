import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { scrollDirections } from 'Helpers/Props';
import styles from './Scroller.css';

class Scroller extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._scroller = null;
  }

  componentDidMount() {
    const {
      scrollDirection,
      autoFocus,
      scrollTop
    } = this.props;

    if (this.props.scrollTop != null) {
      this._scroller.scrollTop = scrollTop;
    }

    if (autoFocus && scrollDirection !== scrollDirections.NONE) {
      this._scroller.focus({ preventScroll: true });
    }
  }

  //
  // Control

  _setScrollerRef = (ref) => {
    this._scroller = ref;

    this.props.registerScroller(ref);
  };

  //
  // Render

  render() {
    const {
      className,
      scrollDirection,
      autoScroll,
      children,
      scrollTop,
      onScroll,
      registerScroller,
      ...otherProps
    } = this.props;

    return (
      <div
        ref={this._setScrollerRef}
        className={classNames(
          className,
          styles.scroller,
          styles[scrollDirection],
          autoScroll && styles.autoScroll
        )}
        tabIndex={-1}
        {...otherProps}
      >
        {children}
      </div>
    );
  }

}

Scroller.propTypes = {
  className: PropTypes.string,
  scrollDirection: PropTypes.oneOf(scrollDirections.all).isRequired,
  autoFocus: PropTypes.bool.isRequired,
  autoScroll: PropTypes.bool.isRequired,
  scrollTop: PropTypes.number,
  children: PropTypes.node,
  onScroll: PropTypes.func,
  registerScroller: PropTypes.func
};

Scroller.defaultProps = {
  scrollDirection: scrollDirections.VERTICAL,
  autoFocus: true,
  autoScroll: true,
  registerScroller: () => { /* no-op */ }
};

export default Scroller;
