import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import TextInput from './TextInput';
import styles from './KeyValueListInputItem.css';

class KeyValueListInputItem extends Component {

  //
  // Listeners

  onKeyChange = ({ value: keyValue }) => {
    const {
      index,
      value,
      onChange
    } = this.props;

    onChange(index, { key: keyValue, value });
  }

  onValueChange = ({ value }) => {
    // TODO: Validate here or validate at a lower level component

    const {
      index,
      keyValue,
      onChange
    } = this.props;

    onChange(index, { key: keyValue, value });
  }

  onRemovePress = () => {
    const {
      index,
      onRemove
    } = this.props;

    onRemove(index);
  }

  onFocus = () => {
    this.props.onFocus();
  }

  onBlur = () => {
    this.props.onBlur();
  }

  //
  // Render

  render() {
    const {
      keyValue,
      value,
      keyPlaceholder,
      valuePlaceholder,
      isNew
    } = this.props;

    return (
      <div className={styles.itemContainer}>
        <TextInput
          className={styles.keyInput}
          name="key"
          value={keyValue}
          placeholder={keyPlaceholder}
          onChange={this.onKeyChange}
          onFocus={this.onFocus}
          onBlur={this.onBlur}
        />

        <TextInput
          className={styles.valueInput}
          name="value"
          value={value}
          placeholder={valuePlaceholder}
          onChange={this.onValueChange}
          onFocus={this.onFocus}
          onBlur={this.onBlur}
        />

        {
          !isNew &&
            <IconButton
              name={icons.REMOVE}
              tabIndex={-1}
              onPress={this.onRemovePress}
            />
        }
      </div>
    );
  }
}

KeyValueListInputItem.propTypes = {
  index: PropTypes.number,
  keyValue: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  keyPlaceholder: PropTypes.string.isRequired,
  valuePlaceholder: PropTypes.string.isRequired,
  isNew: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired,
  onRemove: PropTypes.func.isRequired,
  onFocus: PropTypes.func.isRequired,
  onBlur: PropTypes.func.isRequired
};

KeyValueListInputItem.defaultProps = {
  keyPlaceholder: 'Key',
  valuePlaceholder: 'Value'
};

export default KeyValueListInputItem;
