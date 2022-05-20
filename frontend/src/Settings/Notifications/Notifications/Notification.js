import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
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
  };

  onEditNotificationModalClose = () => {
    this.setState({ isEditNotificationModalOpen: false });
  };

  onDeleteNotificationPress = () => {
    this.setState({
      isEditNotificationModalOpen: false,
      isDeleteNotificationModalOpen: true
    });
  };

  onDeleteNotificationModalClose = () => {
    this.setState({ isDeleteNotificationModalOpen: false });
  };

  onConfirmDeleteNotification = () => {
    this.props.onConfirmDeleteNotification(this.props.id);
  };

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
      onSeriesDelete,
      onEpisodeFileDelete,
      onEpisodeFileDeleteForUpgrade,
      onHealthIssue,
      onApplicationUpdate,
      supportsOnGrab,
      supportsOnDownload,
      supportsOnUpgrade,
      supportsOnRename,
      supportsOnSeriesDelete,
      supportsOnEpisodeFileDelete,
      supportsOnEpisodeFileDeleteForUpgrade,
      supportsOnHealthIssue,
      supportsOnApplicationUpdate
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
          supportsOnGrab && onGrab ?
            <Label kind={kinds.SUCCESS}>
              On Grab
            </Label> :
            null
        }

        {
          supportsOnDownload && onDownload ?
            <Label kind={kinds.SUCCESS}>
              On Import
            </Label> :
            null
        }

        {
          supportsOnUpgrade && onDownload && onUpgrade ?
            <Label kind={kinds.SUCCESS}>
              On Upgrade
            </Label> :
            null
        }

        {
          supportsOnRename && onRename ?
            <Label kind={kinds.SUCCESS}>
              On Rename
            </Label> :
            null
        }

        {
          supportsOnHealthIssue && onHealthIssue ?
            <Label kind={kinds.SUCCESS}>
              On Health Issue
            </Label> :
            null
        }

        {
          supportsOnApplicationUpdate && onApplicationUpdate ?
            <Label kind={kinds.SUCCESS}>
              On Application Update
            </Label> :
            null
        }

        {
          supportsOnSeriesDelete && onSeriesDelete ?
            <Label kind={kinds.SUCCESS}>
              On Series Delete
            </Label> :
            null
        }

        {
          supportsOnEpisodeFileDelete && onEpisodeFileDelete ?
            <Label kind={kinds.SUCCESS}>
              On Episode File Delete
            </Label> :
            null
        }

        {
          supportsOnEpisodeFileDeleteForUpgrade && onEpisodeFileDelete && onEpisodeFileDeleteForUpgrade ?
            <Label kind={kinds.SUCCESS}>
              On Episode File Delete For Upgrade
            </Label> :
            null
        }

        {
          !onGrab && !onDownload && !onRename && !onHealthIssue && !onApplicationUpdate && !onSeriesDelete && !onEpisodeFileDelete ?
            <Label
              kind={kinds.DISABLED}
              outline={true}
            >
              Disabled
            </Label> :
            null
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
  onSeriesDelete: PropTypes.bool.isRequired,
  onEpisodeFileDelete: PropTypes.bool.isRequired,
  onEpisodeFileDeleteForUpgrade: PropTypes.bool.isRequired,
  onHealthIssue: PropTypes.bool.isRequired,
  onApplicationUpdate: PropTypes.bool.isRequired,
  supportsOnGrab: PropTypes.bool.isRequired,
  supportsOnDownload: PropTypes.bool.isRequired,
  supportsOnSeriesDelete: PropTypes.bool.isRequired,
  supportsOnEpisodeFileDelete: PropTypes.bool.isRequired,
  supportsOnEpisodeFileDeleteForUpgrade: PropTypes.bool.isRequired,
  supportsOnUpgrade: PropTypes.bool.isRequired,
  supportsOnRename: PropTypes.bool.isRequired,
  supportsOnHealthIssue: PropTypes.bool.isRequired,
  supportsOnApplicationUpdate: PropTypes.bool.isRequired,
  onConfirmDeleteNotification: PropTypes.func.isRequired
};

export default Notification;
