import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import Portal from 'react-portal';
import classNames from 'classnames';
import elementClass from 'element-class';
import getUniqueElememtId from 'Utilities/getUniqueElementId';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import { sizes } from 'Helpers/Props';
import styles from './Modal.css';

const openModals = [];

function removeFromOpenModals(id) {
  const index = openModals.indexOf(id);

  if (index >= 0) {
    openModals.splice(index, 1);
  }
}

class Modal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._modalId = getUniqueElememtId();
  }

  componentDidMount() {
    if (this.props.isOpen) {
      this._openModal();
    }
  }

  componentDidUpdate(prevProps) {
    const {
      isOpen
    } = this.props;

    if (!prevProps.isOpen && isOpen) {
      this._openModal();
    } else if (prevProps.isOpen && !isOpen) {
      this._closeModal();
    }
  }

  componentWillUnmount() {
    if (this.props.isOpen) {
      this._closeModal();
    }
  }

  //
  // Control

  _openModal() {
    openModals.push(this._modalId);
    window.addEventListener('keydown', this.onKeyDown);

    if (openModals.length === 1) {
      elementClass(document.body).add(styles.modalOpen);
    }
  }

  _closeModal() {
    removeFromOpenModals(this._modalId);
    window.removeEventListener('keydown', this.onKeyDown);

    if (openModals.length === 0) {
      elementClass(document.body).remove(styles.modalOpen);
    }
  }

  _isBackdropTarget(event) {
    const targetElement = this._findEventTarget(event);

    if (targetElement) {
      const modalElement = ReactDOM.findDOMNode(this.refs.modal);

      return !modalElement || !modalElement.contains(targetElement);
    }

    return false;
  }

  _findEventTarget(event) {
    const changedTouches = event.changedTouches;

    if (!changedTouches) {
      return event.target;
    }

    if (changedTouches.length === 1) {
      const touch = changedTouches[0];

      return document.elementFromPoint(touch.clientX, touch.clientY);
    }
  }

  //
  // Listeners

  onBackdropBeginPress = (event) => {
    this._isBackdropPressed = this._isBackdropTarget(event);
  }

  onBackdropEndPress = (event) => {
    if (this._isBackdropPressed && this._isBackdropTarget(event)) {
      this.props.onModalClose();
    }

    this._isBackdropPressed = false;
  }

  onKeyDown = (event) => {
    const keyCode = event.keyCode;

    if (keyCode === keyCodes.ESCAPE) {
      if (openModals.indexOf(this._modalId) === openModals.length - 1) {
        event.preventDefault();
        event.stopPropagation();

        this.props.onModalClose();
      }
    }
  }

  onClosePress = (event) => {
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      className,
      style,
      backdropClassName,
      size,
      children,
      isOpen
    } = this.props;

    return (
      <Portal
        isOpened={isOpen}
      >
        <div>
          {
            isOpen &&
              <div
                className={styles.modalContainer}
              >
                <div
                  className={backdropClassName}
                  onMouseDown={this.onBackdropBeginPress}
                  onMouseUp={this.onBackdropEndPress}
                >
                  <div
                    ref="modal"
                    className={classNames(
                      className,
                      styles[size]
                    )}
                    style={style}
                  >
                    {children}
                  </div>
                </div>
              </div>
          }
        </div>
      </Portal>
    );
  }
}

Modal.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  backdropClassName: PropTypes.string,
  size: PropTypes.oneOf(sizes.all),
  children: PropTypes.node,
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

Modal.defaultProps = {
  className: styles.modal,
  backdropClassName: styles.modalBackdrop,
  size: sizes.LARGE
};

export default Modal;
