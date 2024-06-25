import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
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
      onImportComplete,
      onRename,
      onSeriesAdd,
      onSeriesDelete,
      onEpisodeFileDelete,
      onEpisodeFileDeleteForUpgrade,
      onHealthIssue,
      onHealthRestored,
      onApplicationUpdate,
      onManualInteractionRequired,
      supportsOnGrab,
      supportsOnDownload,
      supportsOnUpgrade,
      supportsOnImportComplete,
      supportsOnRename,
      supportsOnSeriesAdd,
      supportsOnSeriesDelete,
      supportsOnEpisodeFileDelete,
      supportsOnEpisodeFileDeleteForUpgrade,
      supportsOnHealthIssue,
      supportsOnHealthRestored,
      supportsOnApplicationUpdate,
      supportsOnManualInteractionRequired,
      tags,
      tagList
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
              {translate('OnGrab')}
            </Label> :
            null
        }

        {
          supportsOnDownload && onDownload ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnFileImport')}
            </Label> :
            null
        }

        {
          supportsOnUpgrade && onDownload && onUpgrade ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnUpgrade')}
            </Label> :
            null
        }

        {
          supportsOnImportComplete && onImportComplete ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnImportComplete')}
            </Label> :
            null
        }

        {
          supportsOnRename && onRename ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnRename')}
            </Label> :
            null
        }

        {
          supportsOnHealthIssue && onHealthIssue ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnHealthIssue')}
            </Label> :
            null
        }

        {
          supportsOnHealthRestored && onHealthRestored ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnHealthRestored')}
            </Label> :
            null
        }

        {
          supportsOnApplicationUpdate && onApplicationUpdate ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnApplicationUpdate')}
            </Label> :
            null
        }

        {
          supportsOnSeriesAdd && onSeriesAdd ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnSeriesAdd')}
            </Label> :
            null
        }

        {
          supportsOnSeriesDelete && onSeriesDelete ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnSeriesDelete')}
            </Label> :
            null
        }

        {
          supportsOnEpisodeFileDelete && onEpisodeFileDelete ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnEpisodeFileDelete')}
            </Label> :
            null
        }

        {
          supportsOnEpisodeFileDeleteForUpgrade && onEpisodeFileDelete && onEpisodeFileDeleteForUpgrade ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnEpisodeFileDeleteForUpgrade')}
            </Label> :
            null
        }

        {
          supportsOnManualInteractionRequired && onManualInteractionRequired ?
            <Label kind={kinds.SUCCESS}>
              {translate('OnManualInteractionRequired')}
            </Label> :
            null
        }

        {
          !onGrab && !onDownload && !onRename && !onImportComplete && !onHealthIssue && !onHealthRestored && !onApplicationUpdate && !onSeriesAdd && !onSeriesDelete && !onEpisodeFileDelete && !onManualInteractionRequired ?
            <Label
              kind={kinds.DISABLED}
              outline={true}
            >
              {translate('Disabled')}
            </Label> :
            null
        }

        <TagList
          tags={tags}
          tagList={tagList}
        />

        <EditNotificationModalConnector
          id={id}
          isOpen={this.state.isEditNotificationModalOpen}
          onModalClose={this.onEditNotificationModalClose}
          onDeleteNotificationPress={this.onDeleteNotificationPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteNotificationModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteNotification')}
          message={translate('DeleteNotificationMessageText', { name })}
          confirmLabel={translate('Delete')}
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
  onImportComplete: PropTypes.bool.isRequired,
  onRename: PropTypes.bool.isRequired,
  onSeriesAdd: PropTypes.bool.isRequired,
  onSeriesDelete: PropTypes.bool.isRequired,
  onEpisodeFileDelete: PropTypes.bool.isRequired,
  onEpisodeFileDeleteForUpgrade: PropTypes.bool.isRequired,
  onHealthIssue: PropTypes.bool.isRequired,
  onHealthRestored: PropTypes.bool.isRequired,
  onApplicationUpdate: PropTypes.bool.isRequired,
  onManualInteractionRequired: PropTypes.bool.isRequired,
  supportsOnGrab: PropTypes.bool.isRequired,
  supportsOnDownload: PropTypes.bool.isRequired,
  supportsOnImportComplete: PropTypes.bool.isRequired,
  supportsOnSeriesAdd: PropTypes.bool.isRequired,
  supportsOnSeriesDelete: PropTypes.bool.isRequired,
  supportsOnEpisodeFileDelete: PropTypes.bool.isRequired,
  supportsOnEpisodeFileDeleteForUpgrade: PropTypes.bool.isRequired,
  supportsOnUpgrade: PropTypes.bool.isRequired,
  supportsOnRename: PropTypes.bool.isRequired,
  supportsOnHealthIssue: PropTypes.bool.isRequired,
  supportsOnHealthRestored: PropTypes.bool.isRequired,
  supportsOnApplicationUpdate: PropTypes.bool.isRequired,
  supportsOnManualInteractionRequired: PropTypes.bool.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteNotification: PropTypes.func.isRequired
};

export default Notification;
