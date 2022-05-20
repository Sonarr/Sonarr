import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import KeyValueListInputItem from './KeyValueListInputItem';
import styles from './KeyValueListInput.css';

class KeyValueListInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isFocused: false
    };
  }

  //
  // Listeners

  onItemChange = (index, itemValue) => {
    const {
      name,
      value,
      onChange
    } = this.props;

    const newValue = [...value];

    if (index == null) {
      newValue.push(itemValue);
    } else {
      newValue.splice(index, 1, itemValue);
    }

    onChange({
      name,
      value: newValue
    });
  };

  onRemoveItem = (index) => {
    const {
      name,
      value,
      onChange
    } = this.props;

    const newValue = [...value];
    newValue.splice(index, 1);

    onChange({
      name,
      value: newValue
    });
  };

  onFocus = () => {
    this.setState({
      isFocused: true
    });
  };

  onBlur = () => {
    this.setState({
      isFocused: false
    });

    const {
      name,
      value,
      onChange
    } = this.props;

    const newValue = value.reduce((acc, v) => {
      if (v.key || v.value) {
        acc.push(v);
      }

      return acc;
    }, []);

    if (newValue.length !== value.length) {
      onChange({
        name,
        value: newValue
      });
    }
  };

  //
  // Render

  render() {
    const {
      className,
      value,
      keyPlaceholder,
      valuePlaceholder,
      hasError,
      hasWarning
    } = this.props;

    const { isFocused } = this.state;

    return (
      <div className={classNames(
        className,
        isFocused && styles.isFocused,
        hasError && styles.hasError,
        hasWarning && styles.hasWarning
      )}
      >
        {
          [...value, { key: '', value: '' }].map((v, index) => {
            return (
              <KeyValueListInputItem
                key={index}
                index={index}
                keyValue={v.key}
                value={v.value}
                keyPlaceholder={keyPlaceholder}
                valuePlaceholder={valuePlaceholder}
                isNew={index === value.length}
                onChange={this.onItemChange}
                onRemove={this.onRemoveItem}
                onFocus={this.onFocus}
                onBlur={this.onBlur}
              />
            );
          })
        }
      </div>
    );
  }
}

KeyValueListInput.propTypes = {
  className: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  keyPlaceholder: PropTypes.string,
  valuePlaceholder: PropTypes.string,
  onChange: PropTypes.func.isRequired
};

KeyValueListInput.defaultProps = {
  className: styles.inputContainer,
  value: []
};

export default KeyValueListInput;
