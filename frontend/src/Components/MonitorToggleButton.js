import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import styles from './MonitorToggleButton.css';

class MonitorToggleButton extends Component {

  //
  // Listeners

  onPress = (event) => {
    const shiftKey = event.nativeEvent.shiftKey;

    this.props.onPress(!this.props.monitored, { shiftKey });
  }

  //
  // Render

  render() {
    const {
      className,
      monitored,
      isDisabled,
      isSaving,
      size,
      ...otherProps
    } = this.props;

    const monitoredMessage = 'Monitored, click to unmonitor';
    const unmonitoredMessage = 'Unmonitored, click to monitor';
    const iconName = monitored ? icons.MONITORED : icons.UNMONITORED;

    if (isDisabled) {
      return (
        <Icon
          className={styles.disabledButton}
          size={size}
          name={iconName}
          title="Cannot toogle monitored state when series is unmonitored"
        />
      );
    }

    return (
      <SpinnerIconButton
        className={className}
        name={iconName}
        size={size}
        title={monitored ? monitoredMessage : unmonitoredMessage}
        isSpinning={isSaving}
        {...otherProps}
        onPress={this.onPress}
      />
    );
  }
}

MonitorToggleButton.propTypes = {
  className: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  size: PropTypes.number,
  isDisabled: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

MonitorToggleButton.defaultProps = {
  className: styles.toggleButton,
  isDisabled: false,
  isSaving: false
};

export default MonitorToggleButton;
