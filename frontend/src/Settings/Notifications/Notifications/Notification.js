import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import EditNotificationModalConnector from './EditNotificationModalConnector';
import styles from './Notification.css';

class Notification extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditNotificationModalOpen: false,
      isDeleteNotificationModalOpen: false
    };
  }

  //
  // Listeners

  onEditNotificationPress = () => {
    this.setState({ isEditNotificationModalOpen: true });
  }

  onEditNotificationModalClose = () => {
    this.setState({ isEditNotificationModalOpen: false });
  }

  onDeleteNotificationPress = () => {
    this.setState({
      isEditNotificationModalOpen: false,
      isDeleteNotificationModalOpen: true
    });
  }

  onDeleteNotificationModalClose= () => {
    this.setState({ isDeleteNotificationModalOpen: false });
  }

  onConfirmDeleteNotification = () => {
    this.props.onConfirmDeleteNotification(this.props.id);
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      onGrab,
      onDownload,
      onUpgrade,
      onRename,
      onHealthIssue,
      supportsOnGrab,
      supportsOnDownload,
      supportsOnUpgrade,
      supportsOnRename,
      supportsOnHealthIssue
    } = this.props;

    return (
      <Card
        className={styles.notification}
        overlayContent={true}
        onPress={this.onEditNotificationPress}
      >
        <div className={styles.name}>
          {name}
        </div>

        {
          supportsOnGrab && onGrab &&
            <Label kind={kinds.SUCCESS}>
              On Grab
            </Label>
        }

        {
          supportsOnDownload && onDownload &&
            <Label kind={kinds.SUCCESS}>
              On Import
            </Label>
        }

        {
          supportsOnUpgrade && onDownload && onUpgrade &&
            <Label kind={kinds.SUCCESS}>
              On Upgrade
            </Label>
        }

        {
          supportsOnRename && onRename &&
            <Label kind={kinds.SUCCESS}>
              On Rename
            </Label>
        }

        {
          supportsOnHealthIssue && onHealthIssue &&
            <Label kind={kinds.SUCCESS}>
              On Health Issue
            </Label>
        }

        {
          !onGrab && !onDownload && !onRename && !onHealthIssue &&
            <Label
              kind={kinds.DISABLED}
              outline={true}
            >
              Disabled
            </Label>
        }

        <EditNotificationModalConnector
          id={id}
          isOpen={this.state.isEditNotificationModalOpen}
          onModalClose={this.onEditNotificationModalClose}
          onDeleteNotificationPress={this.onDeleteNotificationPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteNotificationModalOpen}
          kind={kinds.DANGER}
          title="Delete Notification"
          message={`Are you sure you want to delete the notification '${name}'?`}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDeleteNotification}
          onCancel={this.onDeleteNotificationModalClose}
        />
      </Card>
    );
  }
}

Notification.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  onGrab: PropTypes.bool.isRequired,
  onDownload: PropTypes.bool.isRequired,
  onUpgrade: PropTypes.bool.isRequired,
  onRename: PropTypes.bool.isRequired,
  onHealthIssue: PropTypes.bool.isRequired,
  supportsOnGrab: PropTypes.bool.isRequired,
  supportsOnDownload: PropTypes.bool.isRequired,
  supportsOnUpgrade: PropTypes.bool.isRequired,
  supportsOnRename: PropTypes.bool.isRequired,
  supportsOnHealthIssue: PropTypes.bool.isRequired,
  onConfirmDeleteNotification: PropTypes.func.isRequired
};

export default Notification;
