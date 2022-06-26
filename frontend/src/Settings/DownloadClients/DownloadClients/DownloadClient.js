import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import EditDownloadClientModalConnector from './EditDownloadClientModalConnector';
import styles from './DownloadClient.css';

class DownloadClient extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditDownloadClientModalOpen: false,
      isDeleteDownloadClientModalOpen: false
    };
  }

  //
  // Listeners

  onEditDownloadClientPress = () => {
    this.setState({ isEditDownloadClientModalOpen: true });
  };

  onEditDownloadClientModalClose = () => {
    this.setState({ isEditDownloadClientModalOpen: false });
  };

  onDeleteDownloadClientPress = () => {
    this.setState({
      isEditDownloadClientModalOpen: false,
      isDeleteDownloadClientModalOpen: true
    });
  };

  onDeleteDownloadClientModalClose= () => {
    this.setState({ isDeleteDownloadClientModalOpen: false });
  };

  onConfirmDeleteDownloadClient = () => {
    this.props.onConfirmDeleteDownloadClient(this.props.id);
  };

  //
  // Render

  render() {
    const {
      id,
      name,
      enable,
      priority
    } = this.props;

    return (
      <Card
        className={styles.downloadClient}
        overlayContent={true}
        onPress={this.onEditDownloadClientPress}
      >
        <div className={styles.name}>
          {name}
        </div>

        <div className={styles.enabled}>
          {
            enable ?
              <Label kind={kinds.SUCCESS}>
                Enabled
              </Label> :
              <Label
                kind={kinds.DISABLED}
                outline={true}
              >
                Disabled
              </Label>
          }

          {
            priority > 1 &&
              <Label
                kind={kinds.DISABLED}
                outline={true}
              >
                Priority: {priority}
              </Label>
          }
        </div>

        <EditDownloadClientModalConnector
          id={id}
          isOpen={this.state.isEditDownloadClientModalOpen}
          onModalClose={this.onEditDownloadClientModalClose}
          onDeleteDownloadClientPress={this.onDeleteDownloadClientPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteDownloadClientModalOpen}
          kind={kinds.DANGER}
          title="Delete Download Client"
          message={`Are you sure you want to delete the download client '${name}'?`}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDeleteDownloadClient}
          onCancel={this.onDeleteDownloadClientModalClose}
        />
      </Card>
    );
  }
}

DownloadClient.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  enable: PropTypes.bool.isRequired,
  priority: PropTypes.number.isRequired,
  onConfirmDeleteDownloadClient: PropTypes.func.isRequired
};

export default DownloadClient;
