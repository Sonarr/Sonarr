import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Manager, Popper, Reference } from 'react-popper';
import Portal from 'Components/Portal';
import { kinds, tooltipPositions } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import { isMobile as isMobileUtil } from 'Utilities/browser';
import styles from './Tooltip.css';

let maxWidth = null;

function getMaxWidth() {
  const windowWidth = window.innerWidth;

  if (windowWidth >= parseInt(dimensions.breakpointLarge)) {
    maxWidth = 800;
  } else if (windowWidth >= parseInt(dimensions.breakpointMedium)) {
    maxWidth = 650;
  } else if (windowWidth >= parseInt(dimensions.breakpointSmall)) {
    maxWidth = 500;
  } else {
    maxWidth = 450;
  }

  return maxWidth;
}

class Tooltip extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._scheduleUpdate = null;
    this._closeTimeout = null;
    this._maxWidth = maxWidth || getMaxWidth();

    this.state = {
      isOpen: false
    };
  }

  componentDidUpdate() {
    if (this._scheduleUpdate && this.state.isOpen) {
      this._scheduleUpdate();
    }
  }

  componentWillUnmount() {
    if (this._closeTimeout) {
      this._closeTimeout = clearTimeout(this._closeTimeout);
    }
  }

  //
  // Control

  computeMaxSize = (data) => {
    const {
      top,
      right,
      bottom,
      left
    } = data.offsets.reference;

    const windowWidth = window.innerWidth;
    const windowHeight = window.innerHeight;

    if ((/^top/).test(data.placement)) {
      data.styles.maxHeight = top - 20;
    } else if ((/^bottom/).test(data.placement)) {
      data.styles.maxHeight = windowHeight - bottom - 20;
    } else if ((/^right/).test(data.placement)) {
      data.styles.maxWidth = Math.min(this._maxWidth, windowWidth - right - 20);
      data.styles.maxHeight = top - 20;
    } else {
      data.styles.maxWidth = Math.min(this._maxWidth, left - 20);
      data.styles.maxHeight = top - 20;
    }

    return data;
  };

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.setState({ width });
  };

  onClick = () => {
    if (isMobileUtil()) {
      this.setState({ isOpen: !this.state.isOpen });
    }
  };

  onMouseEnter = () => {
    if (this._closeTimeout) {
      this._closeTimeout = clearTimeout(this._closeTimeout);
    }

    this.setState({ isOpen: true });
  };

  onMouseLeave = () => {
    this._closeTimeout = setTimeout(() => {
      this.setState({ isOpen: false });
    }, 100);
  };

  //
  // Render

  render() {
    const {
      className,
      bodyClassName,
      anchor,
      tooltip,
      kind,
      position,
      canFlip
    } = this.props;

    return (
      <Manager>
        <Reference>
          {({ ref }) => (
            <span
              ref={ref}
              className={className}
              onClick={this.onClick}
              onMouseEnter={this.onMouseEnter}
              onMouseLeave={this.onMouseLeave}
            >
              {anchor}
            </span>
          )}
        </Reference>

        <Portal>
          <Popper
            placement={position}
            // Disable events to improve performance when many tooltips
            // are shown (Quality Definitions for example).
            eventsEnabled={false}
            modifiers={{
              computeMaxHeight: {
                order: 851,
                enabled: true,
                fn: this.computeMaxSize
              },
              preventOverflow: {
                // Fixes positioning for tooltips in the queue
                // and likely others.
                escapeWithReference: false
              },
              flip: {
                enabled: canFlip
              }
            }}
          >
            {({ ref, style, placement, arrowProps, scheduleUpdate }) => {
              this._scheduleUpdate = scheduleUpdate;

              const popperPlacement = placement ? placement.split('-')[0] : position;
              const vertical = popperPlacement === 'top' || popperPlacement === 'bottom';

              return (
                <div
                  ref={ref}
                  className={classNames(
                    styles.tooltipContainer,
                    vertical ? styles.verticalContainer : styles.horizontalContainer
                  )}
                  style={style}
                  onMouseEnter={this.onMouseEnter}
                  onMouseLeave={this.onMouseLeave}
                >
                  <div
                    className={this.state.isOpen ? classNames(
                      styles.arrow,
                      styles[kind],
                      styles[popperPlacement]
                    ) : styles.arrowDisabled}
                    ref={arrowProps.ref}
                    style={arrowProps.style}
                  />
                  {
                    this.state.isOpen ?
                      <div
                        className={classNames(
                          styles.tooltip,
                          styles[kind]
                        )}
                      >
                        <div
                          className={bodyClassName}
                        >
                          {tooltip}
                        </div>
                      </div> :
                      null
                  }
                </div>
              );
            }}
          </Popper>
        </Portal>
      </Manager>
    );
  }
}

Tooltip.propTypes = {
  className: PropTypes.string,
  bodyClassName: PropTypes.string.isRequired,
  anchor: PropTypes.node.isRequired,
  tooltip: PropTypes.oneOfType([PropTypes.string, PropTypes.node]).isRequired,
  kind: PropTypes.oneOf([kinds.DEFAULT, kinds.INVERSE]),
  position: PropTypes.oneOf(tooltipPositions.all),
  canFlip: PropTypes.bool.isRequired
};

Tooltip.defaultProps = {
  bodyClassName: styles.body,
  kind: kinds.DEFAULT,
  position: tooltipPositions.TOP,
  canFlip: false
};

export default Tooltip;
