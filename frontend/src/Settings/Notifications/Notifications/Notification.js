import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import EditNotificationModalConnector from './EditNotificationModalConnector';
import styles from './Notification.css';

function getLabelKind(supports, enabled) {
  if (!supports) {
    return kinds.DEFAULT;
  }

  if (!enabled) {
    return kinds.DANGER;
  }

  return kinds.SUCCESS;
}

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
      supportsOnGrab,
      supportsOnDownload,
      supportsOnUpgrade,
      supportsOnRename
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

        <Label
          kind={getLabelKind(supportsOnGrab, onGrab)}
          outline={supportsOnGrab && !onGrab}
        >
          On Grab
        </Label>

        <Label
          kind={getLabelKind(supportsOnDownload, onDownload)}
          outline={supportsOnDownload && !onDownload}
        >
          On Download
        </Label>

        <Label
          kind={getLabelKind(supportsOnUpgrade, onDownload && onUpgrade)}
          outline={supportsOnUpgrade && !(onDownload && onUpgrade)}
        >
          On Upgrade
        </Label>

        <Label
          kind={getLabelKind(supportsOnRename, onRename)}
          outline={supportsOnRename && !onRename}
        >
          On Rename
        </Label>

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
  supportsOnGrab: PropTypes.bool.isRequired,
  supportsOnDownload: PropTypes.bool.isRequired,
  supportsOnUpgrade: PropTypes.bool.isRequired,
  supportsOnRename: PropTypes.bool.isRequired,
  onConfirmDeleteNotification: PropTypes.func.isRequired
};

export default Notification;
