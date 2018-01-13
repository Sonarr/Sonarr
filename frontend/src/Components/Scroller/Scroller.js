import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
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
      scrollTop
    } = this.props;

    if (this.props.scrollTop != null) {
      this._scroller.scrollTop = scrollTop;
    }
  }

  //
  // Control

  _setScrollerRef = (ref) => {
    this._scroller = ref;
  }

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
        {...otherProps}
      >
        {children}
      </div>
    );
  }

}

Scroller.propTypes = {
  className: PropTypes.string,
  scrollDirection: PropTypes.oneOf([scrollDirections.NONE, scrollDirections.HORIZONTAL, scrollDirections.VERTICAL]).isRequired,
  autoScroll: PropTypes.bool.isRequired,
  scrollTop: PropTypes.number,
  children: PropTypes.node,
  onScroll: PropTypes.func
};

Scroller.defaultProps = {
  scrollDirection: scrollDirections.VERTICAL,
  autoScroll: true
};

export default Scroller;
