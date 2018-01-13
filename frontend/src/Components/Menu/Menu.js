import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import TetherComponent from 'react-tether';
import { align } from 'Helpers/Props';
import styles from './Menu.css';

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
  [align.RIGHT]: {
    ...baseTetherOptions,
    attachment: 'top right',
    targetAttachment: 'bottom right'
  },

  [align.LEFT]: {
    ...baseTetherOptions,
    attachment: 'top left',
    targetAttachment: 'bottom left'
  }
};

class Menu extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isMenuOpen: false,
      maxHeight: 0
    };
  }

  componentDidMount() {
    this.setMaxHeight();
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

    const menu = ReactDOM.findDOMNode(this.refs.menu);

    if (!menu) {
      return;
    }

    const { bottom } = menu.getBoundingClientRect();
    const maxHeight = window.innerHeight - bottom;

    return maxHeight;
  }

  setMaxHeight() {
    this.setState({
      maxHeight: this.getMaxHeight()
    });
  }

  _addListener() {
    // Listen to resize events on the window and scroll events
    // on all elements to ensure the menu is the best size possible.
    // Listen for click events on the window to support closing the
    // menu on clicks outside.

    window.addEventListener('resize', this.onWindowResize);
    window.addEventListener('scroll', this.onWindowScroll, { capture: true });
    window.addEventListener('click', this.onWindowClick);
  }

  _removeListener() {
    window.removeEventListener('resize', this.onWindowResize);
    window.removeEventListener('scroll', this.onWindowScroll, { capture: true });
    window.removeEventListener('click', this.onWindowClick);
  }

  //
  // Listeners

  onWindowClick = (event) => {
    const menu = ReactDOM.findDOMNode(this.refs.menu);
    const menuContent = ReactDOM.findDOMNode(this.refs.menuContent);

    if (!menu) {
      return;
    }

    if ((!menu.contains(event.target) || menuContent.contains(event.target)) && this.state.isMenuOpen) {
      this.setState({ isMenuOpen: false });
      this._removeListener();
    }
  }

  onWindowResize = () => {
    this.setMaxHeight();
  }

  onWindowScroll = () => {
    this.setMaxHeight();
  }

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
  }

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

    const content = React.cloneElement(
      childrenArray[1],
      {
        ref: 'menuContent',
        alignMenu,
        maxHeight,
        isOpen: isMenuOpen
      }
    );

    return (
      <TetherComponent
        classes={{
          element: styles.tether
        }}
        {...tetherOptions[alignMenu]}
      >
        <div
          ref="menu"
          className={className}
        >
          {button}
        </div>

        {
          isMenuOpen &&
            content
        }
      </TetherComponent>
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
