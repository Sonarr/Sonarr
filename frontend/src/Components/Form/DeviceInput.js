import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import tagShape from 'Helpers/Props/Shapes/tagShape';
import FormInputButton from './FormInputButton';
import TagInput from './TagInput';
import styles from './DeviceInput.css';

class DeviceInput extends Component {

  onTagAdd = (device) => {
    const {
      name,
      value,
      onChange
    } = this.props;

    // New tags won't have an ID, only a name.
    const deviceId = device.id || device.name;

    onChange({
      name,
      value: [...value, deviceId]
    });
  };

  onTagDelete = ({ index }) => {
    const {
      name,
      value,
      onChange
    } = this.props;

    const newValue = value.slice();
    newValue.splice(index, 1);

    onChange({
      name,
      value: newValue
    });
  };

  //
  // Render

  render() {
    const {
      className,
      name,
      items,
      selectedDevices,
      hasError,
      hasWarning,
      isFetching,
      onRefreshPress
    } = this.props;

    return (
      <div className={className}>
        <TagInput
          inputContainerClassName={styles.input}
          name={name}
          tags={selectedDevices}
          tagList={items}
          allowNew={true}
          minQueryLength={0}
          hasError={hasError}
          hasWarning={hasWarning}
          onTagAdd={this.onTagAdd}
          onTagDelete={this.onTagDelete}
        />

        <FormInputButton
          onPress={onRefreshPress}
        >
          <Icon
            name={icons.REFRESH}
            isSpinning={isFetching}
          />
        </FormInputButton>
      </div>
    );
  }
}

DeviceInput.propTypes = {
  className: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.arrayOf(PropTypes.oneOfType([PropTypes.number, PropTypes.string])).isRequired,
  items: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  selectedDevices: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired
};

DeviceInput.defaultProps = {
  className: styles.deviceInputWrapper,
  inputClassName: styles.input
};

export default DeviceInput;
