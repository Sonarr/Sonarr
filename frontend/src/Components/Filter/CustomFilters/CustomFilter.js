import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import styles from './CustomFilter.css';

class CustomFilter extends Component {

  //
  // Listeners

  onEditPress = () => {
    const {
      customFilterKey,
      onEditPress
    } = this.props;

    onEditPress(customFilterKey);
  }

  onRemovePress = () => {
    const {
      customFilterKey,
      onRemovePress
    } = this.props;

    onRemovePress({ key: customFilterKey });
  }

  //
  // Render

  render() {
    const {
      label
    } = this.props;

    return (
      <div className={styles.customFilter}>
        <div className={styles.label}>
          {label}
        </div>

        <div className={styles.actions}>
          <IconButton
            name={icons.EDIT}
            onPress={this.onEditPress}
          />

          <IconButton
            name={icons.REMOVE}
            onPress={this.onRemovePress}
          />
        </div>
      </div>
    );
  }
}

CustomFilter.propTypes = {
  customFilterKey: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  onEditPress: PropTypes.func.isRequired,
  onRemovePress: PropTypes.func.isRequired
};

export default CustomFilter;
