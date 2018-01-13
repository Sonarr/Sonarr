import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TetherComponent from 'react-tether';
import classNames from 'classnames';
import { tooltipPositions } from 'Helpers/Props';
import styles from './Popover.css';

const baseTetherOptions = {
  skipMoveElement: true,
  constraints: [
    {
      to: 'window',
      attachment: 'together',
      pin: true
    }
  ]
};

const tetherOptions = {
  [tooltipPositions.TOP]: {
    ...baseTetherOptions,
    attachment: 'bottom center',
    targetAttachment: 'top center'
  },

  [tooltipPositions.RIGHT]: {
    ...baseTetherOptions,
    attachment: 'middle left',
    targetAttachment: 'middle right'
  },

  [tooltipPositions.BOTTOM]: {
    ...baseTetherOptions,
    attachment: 'top center',
    targetAttachment: 'bottom center'
  },

  [tooltipPositions.LEFT]: {
    ...baseTetherOptions,
    attachment: 'middle right',
    targetAttachment: 'middle left'
  }
};

class Popover extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOpen: false
    };

    this._closeTimeout = null;
  }

  componentWillUnmount() {
    if (this._closeTimeout) {
      this._closeTimeout = clearTimeout(this._closeTimeout);
    }
  }

  //
  // Listeners

  onClick = () => {
    this.setState({ isOpen: !this.state.isOpen });
  }

  onMouseEnter = () => {
    if (this._closeTimeout) {
      this._closeTimeout = clearTimeout(this._closeTimeout);
    }

    this.setState({ isOpen: true });
  }

  onMouseLeave = () => {
    this._closeTimeout = setTimeout(() => {
      this.setState({ isOpen: false });
    }, 100);
  }

  //
  // Render

  render() {
    const {
      className,
      anchor,
      title,
      body,
      position
    } = this.props;

    return (
      <TetherComponent
        classes={{
          element: styles.tether
        }}
        {...tetherOptions[position]}
      >
        <span
          className={className}
          // onClick={this.onClick}
          onMouseEnter={this.onMouseEnter}
          onMouseLeave={this.onMouseLeave}
        >
          {anchor}
        </span>

        {
          this.state.isOpen &&
            <div
              className={styles.popoverContainer}
              onMouseEnter={this.onMouseEnter}
              onMouseLeave={this.onMouseLeave}
            >
              <div className={styles.popover}>
                <div
                  className={classNames(
                    styles.arrow,
                    styles[position]
                  )}
                />

                <div className={styles.title}>
                  {title}
                </div>

                <div className={styles.body}>
                  {body}
                </div>
              </div>
            </div>
        }
      </TetherComponent>
    );
  }
}

Popover.propTypes = {
  className: PropTypes.string,
  anchor: PropTypes.node.isRequired,
  title: PropTypes.string.isRequired,
  body: PropTypes.oneOfType([PropTypes.string, PropTypes.node]).isRequired,
  position: PropTypes.oneOf(tooltipPositions.all)
};

Popover.defaultProps = {
  position: tooltipPositions.TOP
};

export default Popover;
