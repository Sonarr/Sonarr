import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import styles from './RestoreBackupModalContent.css';

function getErrorMessage(error) {
  if (!error || !error.responseJSON || !error.responseJSON.message) {
    return 'Error restoring backup';
  }

  return error.responseJSON.message;
}

function getStepIconProps(isExecuting, hasExecuted, error) {
  if (isExecuting) {
    return {
      name: icons.SPINNER,
      isSpinning: true
    };
  }

  if (hasExecuted) {
    return {
      name: icons.CHECK,
      kind: kinds.SUCCESS
    };
  }

  if (error) {
    return {
      name: icons.FATAL,
      kinds: kinds.DANGER,
      title: getErrorMessage(error)
    };
  }

  return {
    name: icons.PENDING
  };
}

class RestoreBackupModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      file: null,
      path: '',
      isRestored: false,
      isRestarted: false,
      isReloading: false
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isRestoring,
      restoreError,
      isRestarting,
      dispatchRestart
    } = this.props;

    if (prevProps.isRestoring && !isRestoring && !restoreError) {
      this.setState({ isRestored: true }, () => {
        dispatchRestart();
      });
    }

    if (prevProps.isRestarting && !isRestarting) {
      this.setState({
        isRestarted: true,
        isReloading: true
      }, () => {
        location.reload();
      });
    }
  }

  //
  // Listeners

  onPathChange = ({ value, files }) => {
    this.setState({
      file: files[0],
      path: value
    });
  }

  onRestorePress = () => {
    const {
      id,
      onRestorePress
    } = this.props;

    onRestorePress({
      id,
      file: this.state.file
    });
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      isRestoring,
      restoreError,
      isRestarting,
      onModalClose
    } = this.props;

    const {
      path,
      isRestored,
      isRestarted,
      isReloading
    } = this.state;

    const isRestoreDisabled = (
      (!id && !path) ||
      isRestoring ||
      isRestarting ||
      isReloading
    );

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Restore Backup
        </ModalHeader>

        <ModalBody>
          {
            !!id && `Would you like to restore the backup '${name}'?`
          }

          {
            !id &&
              <TextInput
                type="file"
                name="path"
                value={path}
                onChange={this.onPathChange}
              />
          }

          <div className={styles.steps}>
            <div className={styles.step}>
              <div className={styles.stepState}>
                <Icon
                  size={20}
                  {...getStepIconProps(isRestoring, isRestored, restoreError)}
                />
              </div>

              <div>Restore</div>
            </div>

            <div className={styles.step}>
              <div className={styles.stepState}>
                <Icon
                  size={20}
                  {...getStepIconProps(isRestarting, isRestarted)}
                />
              </div>

              <div>Restart</div>
            </div>

            <div className={styles.step}>
              <div className={styles.stepState}>
                <Icon
                  size={20}
                  {...getStepIconProps(isReloading, false)}
                />
              </div>

              <div>Reload</div>
            </div>
          </div>
        </ModalBody>

        <ModalFooter>
          <div className={styles.additionalInfo}>
            Note: Sonarr will automatically restart and reload the UI during the restore process.
          </div>

          <Button onPress={onModalClose}>
              Cancel
          </Button>

          <SpinnerButton
            kind={kinds.WARNING}
            isDisabled={isRestoreDisabled}
            isSpinning={isRestoring}
            onPress={this.onRestorePress}
          >
              Restore
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

RestoreBackupModalContent.propTypes = {
  id: PropTypes.number,
  name: PropTypes.string,
  path: PropTypes.string,
  isRestoring: PropTypes.bool.isRequired,
  restoreError: PropTypes.object,
  isRestarting: PropTypes.bool.isRequired,
  dispatchRestart: PropTypes.func.isRequired,
  onRestorePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RestoreBackupModalContent;
