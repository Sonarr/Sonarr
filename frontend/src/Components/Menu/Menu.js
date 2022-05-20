import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Manager, Popper, Reference } from 'react-popper';
import Portal from 'Components/Portal';
import { align } from 'Helpers/Props';
import getUniqueElememtId from 'Utilities/getUniqueElementId';
import styles from './Menu.css';

const sharedPopperOptions = {
  modifiers: {
    preventOverflow: {
      padding: 0
    },
    flip: {
      padding: 0
    }
  }
};

const popperOptions = {
  [align.RIGHT]: {
    ...sharedPopperOptions,
    placement: 'bottom-end'
  },

  [align.LEFT]: {
    ...sharedPopperOptions,
    placement: 'bottom-start'
  }
};

class Menu extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._scheduleUpdate = null;
    this._menuButtonId = getUniqueElememtId();
    this._menuContentId = getUniqueElememtId();

    this.state = {
      isMenuOpen: false,
      maxHeight: 0
    };
  }

  componentDidMount() {
    this.setMaxHeight();
  }

  componentDidUpdate() {
    if (this._scheduleUpdate) {
      this._scheduleUpdate();
    }
  }

  componentWillUnmount() {
    this._removeListener();
  }

  //
  // Control

  getMaxHeight() {
    if (!this.props.enforceMaxHeight) {
      return;
    }

    const menuButton = document.getElementById(this._menuButtonId);

    if (!menuButton) {
      return;
    }

    const { bottom } = menuButton.getBoundingClientRect();
    const maxHeight = window.innerHeight - bottom;

    return maxHeight;
  }

  setMaxHeight() {
    const maxHeight = this.getMaxHeight();

    if (maxHeight !== this.state.maxHeight) {
      this.setState({
        maxHeight
      });
    }
  }

  _addListener() {
    // Listen to resize events on the window and scroll events
    // on all elements to ensure the menu is the best size possible.
    // Listen for click events on the window to support closing the
    // menu on clicks outside.

    window.addEventListener('resize', this.onWindowResize);
    window.addEventListener('scroll', this.onWindowScroll, { capture: true });
    window.addEventListener('click', this.onWindowClick);
    window.addEventListener('touchstart', this.onTouchStart);
  }

  _removeListener() {
    window.removeEventListener('resize', this.onWindowResize);
    window.removeEventListener('scroll', this.onWindowScroll, { capture: true });
    window.removeEventListener('click', this.onWindowClick);
    window.removeEventListener('touchstart', this.onTouchStart);
  }

  //
  // Listeners

  onWindowClick = (event) => {
    const menuButton = document.getElementById(this._menuButtonId);

    if (!menuButton) {
      return;
    }

    if (!menuButton.contains(event.target) && this.state.isMenuOpen) {
      this.setState({ isMenuOpen: false });
      this._removeListener();
    }
  };

  onTouchStart = (event) => {
    const menuButton = document.getElementById(this._menuButtonId);
    const menuContent = document.getElementById(this._menuContentId);

    if (!menuButton || !menuContent) {
      return;
    }

    if (event.targetTouches.length !== 1) {
      return;
    }

    const target = event.targetTouches[0].target;

    if (
      !menuButton.contains(target) &&
      !menuContent.contains(target) &&
      this.state.isMenuOpen
    ) {
      this.setState({ isMenuOpen: false });
      this._removeListener();
    }
  };

  onWindowResize = () => {
    this.setMaxHeight();
  };

  onWindowScroll = (event) => {
    if (this.state.isMenuOpen) {
      this.setMaxHeight();
    }
  };

  onMenuButtonPress = () => {
    const state = {
      isMenuOpen: !this.state.isMenuOpen
    };

    if (this.state.isMenuOpen) {
      this._removeListener();
    } else {
      state.maxHeight = this.getMaxHeight();
      this._addListener();
    }

    this.setState(state);
  };

  //
  // Render

  render() {
    const {
      className,
      children,
      alignMenu
    } = this.props;

    const {
      maxHeight,
      isMenuOpen
    } = this.state;

    const childrenArray = React.Children.toArray(children);
    const button = React.cloneElement(
      childrenArray[0],
      {
        onPress: this.onMenuButtonPress
      }
    );

    return (
      <Manager>
        <Reference>
          {({ ref }) => (
            <div
              ref={ref}
              id={this._menuButtonId}
              className={className}
            >
              {button}
            </div>
          )}
        </Reference>

        <Portal>
          <Popper {...popperOptions[alignMenu]}>
            {({ ref, style, scheduleUpdate }) => {
              this._scheduleUpdate = scheduleUpdate;

              return React.cloneElement(
                childrenArray[1],
                {
                  forwardedRef: ref,
                  style: {
                    ...style,
                    maxHeight
                  },
                  isOpen: isMenuOpen
                }
              );
            }}
          </Popper>
        </Portal>
      </Manager>
    );
  }
}

Menu.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired,
  alignMenu: PropTypes.oneOf([align.LEFT, align.RIGHT]),
  enforceMaxHeight: PropTypes.bool.isRequired
};

Menu.defaultProps = {
  className: styles.menu,
  alignMenu: align.LEFT,
  enforceMaxHeight: true
};

export default Menu;
