import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import { icons, kinds } from 'Helpers/Props';
import FormInputHelpText from './FormInputHelpText';
import styles from './CheckInput.css';

class CheckInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._checkbox = null;
  }

  componentDidMount() {
    this.setIndeterminate();
  }

  componentDidUpdate() {
    this.setIndeterminate();
  }

  //
  // Control

  setIndeterminate() {
    if (!this._checkbox) {
      return;
    }

    const {
      value,
      uncheckedValue,
      checkedValue
    } = this.props;

    this._checkbox.indeterminate = value !== uncheckedValue && value !== checkedValue;
  }

  toggleChecked = (checked, shiftKey) => {
    const {
      name,
      value,
      checkedValue,
      uncheckedValue
    } = this.props;

    const newValue = checked ? checkedValue : uncheckedValue;

    if (value !== newValue) {
      this.props.onChange({
        name,
        value: newValue,
        shiftKey
      });
    }
  };

  //
  // Listeners

  setRef = (ref) => {
    this._checkbox = ref;
  };

  onClick = (event) => {
    if (this.props.isDisabled) {
      return;
    }

    const shiftKey = event.nativeEvent.shiftKey;
    const checked = !this._checkbox.checked;

    event.preventDefault();
    this.toggleChecked(checked, shiftKey);
  };

  onChange = (event) => {
    const checked = event.target.checked;
    const shiftKey = event.nativeEvent.shiftKey;

    this.toggleChecked(checked, shiftKey);
  };

  //
  // Render

  render() {
    const {
      className,
      containerClassName,
      name,
      value,
      checkedValue,
      uncheckedValue,
      helpText,
      helpTextWarning,
      isDisabled,
      kind
    } = this.props;

    const isChecked = value === checkedValue;
    const isUnchecked = value === uncheckedValue;
    const isIndeterminate = !isChecked && !isUnchecked;
    const isCheckClass = `${kind}IsChecked`;

    return (
      <div className={containerClassName}>
        <label
          className={styles.label}
          onClick={this.onClick}
        >
          <input
            ref={this.setRef}
            className={styles.checkbox}
            type="checkbox"
            name={name}
            checked={isChecked}
            disabled={isDisabled}
            onChange={this.onChange}
          />

          <div
            className={classNames(
              className,
              isChecked ? styles[isCheckClass] : styles.isNotChecked,
              isIndeterminate && styles.isIndeterminate,
              isDisabled && styles.isDisabled
            )}
          >
            {
              isChecked &&
                <Icon name={icons.CHECK} />
            }

            {
              isIndeterminate &&
                <Icon name={icons.CHECK_INDETERMINATE} />
            }
          </div>

          {
            helpText &&
              <FormInputHelpText
                className={styles.helpText}
                text={helpText}
              />
          }

          {
            !helpText && helpTextWarning &&
              <FormInputHelpText
                className={styles.helpText}
                text={helpTextWarning}
                isWarning={true}
              />
          }
        </label>
      </div>
    );
  }
}

CheckInput.propTypes = {
  className: PropTypes.string.isRequired,
  containerClassName: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  checkedValue: PropTypes.bool,
  uncheckedValue: PropTypes.bool,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.bool]),
  helpText: PropTypes.string,
  helpTextWarning: PropTypes.string,
  isDisabled: PropTypes.bool,
  kind: PropTypes.oneOf(kinds.all).isRequired,
  onChange: PropTypes.func.isRequired
};

CheckInput.defaultProps = {
  className: styles.input,
  containerClassName: styles.container,
  checkedValue: true,
  uncheckedValue: false,
  kind: kinds.PRIMARY
};

export default CheckInput;
