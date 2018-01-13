import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import styles from './TextInput.css';

class TextInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._input = null;
    this._selectionStart = null;
    this._selectionEnd = null;
    this._selectionTimeout = null;
    this._isMouseTarget = false;
  }

  componentDidMount() {
    window.addEventListener('mouseup', this.onDocumentMouseUp);
  }

  componentWillUnmount() {
    window.removeEventListener('mouseup', this.onDocumentMouseUp);
  }

  //
  // Control

  setInputRef = (ref) => {
    this._input = ref;
  }

  selectionChange() {
    if (this._selectionTimeout) {
      this._selectionTimeout = clearTimeout(this._selectionTimeout);
    }

    this._selectionTimeout = setTimeout(() => {
      const selectionStart = this._input.selectionStart;
      const selectionEnd = this._input.selectionEnd;

      const selectionChanged = (
        this._selectionStart !== selectionStart ||
        this._selectionEnd !== selectionEnd
      );

      this._selectionStart = selectionStart;
      this._selectionEnd = selectionEnd;

      if (this.props.onSelectionChange && selectionChanged) {
        this.props.onSelectionChange(selectionStart, selectionEnd);
      }
    }, 10);
  }

  //
  // Listeners

  onChange = (event) => {
    const {
      name,
      type,
      onChange
    } = this.props;

    const payload = {
      name,
      value: event.target.value
    };

    // Also return the files for a file input type.

    if (type === 'file') {
      payload.files = event.target.files;
    }

    onChange(payload);
  }

  onFocus = (event) => {
    if (this.props.onFocus) {
      this.props.onFocus(event);
    }

    this.selectionChange();
  }

  onKeyUp = () => {
    this.selectionChange();
  }

  onMouseDown = () => {
    this._isMouseTarget = true;
  }

  onMouseUp = () => {
    this.selectionChange();
  }

  onDocumentMouseUp = () => {
    if (this._isMouseTarget) {
      this.selectionChange();
    }

    this._isMouseTarget = false;
  }

  //
  // Render

  render() {
    const {
      className,
      type,
      readOnly,
      autoFocus,
      placeholder,
      name,
      value,
      hasError,
      hasWarning,
      hasButton,
      onBlur
    } = this.props;

    return (
      <input
        ref={this.setInputRef}
        type={type}
        readOnly={readOnly}
        autoFocus={autoFocus}
        placeholder={placeholder}
        className={classNames(
          className,
          readOnly && styles.readOnly,
          hasError && styles.hasError,
          hasWarning && styles.hasWarning,
          hasButton && styles.hasButton
        )}
        name={name}
        value={value}
        onChange={this.onChange}
        onFocus={this.onFocus}
        onBlur={onBlur}
        onKeyUp={this.onKeyUp}
        onMouseDown={this.onMouseDown}
        onMouseUp={this.onMouseUp}
      />
    );
  }
}

TextInput.propTypes = {
  className: PropTypes.string.isRequired,
  type: PropTypes.string.isRequired,
  readOnly: PropTypes.bool,
  autoFocus: PropTypes.bool,
  placeholder: PropTypes.string,
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.array]).isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  hasButton: PropTypes.bool,
  onChange: PropTypes.func.isRequired,
  onFocus: PropTypes.func,
  onBlur: PropTypes.func,
  onSelectionChange: PropTypes.func
};

TextInput.defaultProps = {
  className: styles.text,
  type: 'text',
  readOnly: false,
  autoFocus: false,
  value: ''
};

export default TextInput;
