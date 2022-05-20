import classNames from 'classnames';
import elementClass from 'element-class';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import FocusLock from 'react-focus-lock';
import ErrorBoundary from 'Components/Error/ErrorBoundary';
import { sizes } from 'Helpers/Props';
import { isIOS } from 'Utilities/browser';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import getUniqueElememtId from 'Utilities/getUniqueElementId';
import { setScrollLock } from 'Utilities/scrollLock';
import ModalError from './ModalError';
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

    this._node = document.getElementById('portal-root');
    this._backgroundRef = null;
    this._modalId = getUniqueElememtId();
    this._bodyScrollTop = 0;
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

  _setBackgroundRef = (ref) => {
    this._backgroundRef = ref;
  };

  _openModal() {
    openModals.push(this._modalId);
    window.addEventListener('keydown', this.onKeyDown);

    if (openModals.length === 1) {
      if (isIOS()) {
        setScrollLock(true);
        const scrollTop = document.body.scrollTop;
        this._bodyScrollTop = scrollTop;
        elementClass(document.body).add(styles.modalOpenIOS);
      } else {
        elementClass(document.body).add(styles.modalOpen);
      }
    }
  }

  _closeModal() {
    removeFromOpenModals(this._modalId);
    window.removeEventListener('keydown', this.onKeyDown);

    if (openModals.length === 0) {
      setScrollLock(false);

      if (isIOS()) {
        elementClass(document.body).remove(styles.modalOpenIOS);
        document.body.scrollTop = this._bodyScrollTop;
      } else {
        elementClass(document.body).remove(styles.modalOpen);
      }
    }
  }

  _isBackdropTarget(event) {
    const targetElement = this._findEventTarget(event);

    if (targetElement) {
      const backgroundElement = ReactDOM.findDOMNode(this._backgroundRef);

      return backgroundElement.isEqualNode(targetElement);
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
  };

  onBackdropEndPress = (event) => {
    const {
      closeOnBackgroundClick,
      onModalClose
    } = this.props;

    if (
      this._isBackdropPressed &&
      this._isBackdropTarget(event) &&
      closeOnBackgroundClick
    ) {
      onModalClose();
    }

    this._isBackdropPressed = false;
  };

  onKeyDown = (event) => {
    const keyCode = event.keyCode;

    if (keyCode === keyCodes.ESCAPE) {
      if (openModals.indexOf(this._modalId) === openModals.length - 1) {
        event.preventDefault();
        event.stopPropagation();

        this.props.onModalClose();
      }
    }
  };

  //
  // Render

  render() {
    const {
      className,
      style,
      backdropClassName,
      size,
      children,
      isOpen,
      onModalClose
    } = this.props;

    if (!isOpen) {
      return null;
    }

    return ReactDOM.createPortal(
      <FocusLock disabled={false}>
        <div
          className={styles.modalContainer}
        >
          <div
            ref={this._setBackgroundRef}
            className={backdropClassName}
            onMouseDown={this.onBackdropBeginPress}
            onMouseUp={this.onBackdropEndPress}
          >
            <div
              className={classNames(
                className,
                styles[size]
              )}
              style={style}
            >
              <ErrorBoundary
                errorComponent={ModalError}
                onModalClose={onModalClose}
              >
                {children}
              </ErrorBoundary>
            </div>
          </div>
        </div>
      </FocusLock>,
      this._node
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
  closeOnBackgroundClick: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

Modal.defaultProps = {
  className: styles.modal,
  backdropClassName: styles.modalBackdrop,
  size: sizes.LARGE,
  closeOnBackgroundClick: true
};

export default Modal;
