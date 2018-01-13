import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Clipboard from 'clipboard';
import { icons, kinds } from 'Helpers/Props';
import getUniqueElememtId from 'Utilities/getUniqueElementId';
import Icon from 'Components/Icon';
import FormInputButton from 'Components/Form/FormInputButton';
import styles from './ClipboardButton.css';

class ClipboardButton extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._id = getUniqueElememtId();
    this._successTimeout = null;

    this.state = {
      showSuccess: false,
      showError: false
    };
  }

  componentDidMount() {
    this._clipboard = new Clipboard(`#${this._id}`, {
      text: () => this.props.value
    });

    this._clipboard.on('success', this.onSuccess);
  }

  componentDidUpdate() {
    const {
      showSuccess,
      showError
    } = this.state;

    if (showSuccess || showError) {
      this._testResultTimeout = setTimeout(this.resetState, 3000);
    }
  }

  componentWillUnmount() {
    if (this._clipboard) {
      this._clipboard.destroy();
    }
  }

  //
  // Control

  resetState = () => {
    this.setState({
      showSuccess: false,
      showError: false
    });
  }

  //
  // Listeners

  onSuccess = () => {
    this.setState({
      showSuccess: true
    });
  }

  onError = () => {
    this.setState({
      showError: true
    });
  }

  //
  // Render

  render() {
    const {
      value,
      ...otherProps
    } = this.props;

    const {
      showSuccess,
      showError
    } = this.state;

    const showStateIcon = showSuccess || showError;
    const iconName = showError ? icons.DANGER : icons.CHECK;
    const iconKind = showError ? kinds.DANGER : kinds.SUCCESS;

    return (
      <FormInputButton
        id={this._id}
        className={styles.button}
        {...otherProps}
      >
        <span className={showStateIcon ? styles.showStateIcon : undefined}>
          {
            showSuccess &&
              <span className={styles.stateIconContainer}>
                <Icon
                  name={iconName}
                  kind={iconKind}
                />
              </span>
          }

          {
            <span className={styles.clipboardIconContainer}>
              <Icon name={icons.CLIPBOARD} />
            </span>
          }
        </span>
      </FormInputButton>
    );
  }
}

ClipboardButton.propTypes = {
  value: PropTypes.string.isRequired
};

export default ClipboardButton;
