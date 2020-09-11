import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds } from 'Helpers/Props';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import EditIndexerModalConnector from './EditIndexerModalConnector';
import styles from './Indexer.css';

class Indexer extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditIndexerModalOpen: false,
      isDeleteIndexerModalOpen: false
    };
  }

  //
  // Listeners

  onEditIndexerPress = () => {
    this.setState({ isEditIndexerModalOpen: true });
  }

  onEditIndexerModalClose = () => {
    this.setState({ isEditIndexerModalOpen: false });
  }

  onDeleteIndexerPress = () => {
    this.setState({
      isEditIndexerModalOpen: false,
      isDeleteIndexerModalOpen: true
    });
  }

  onDeleteIndexerModalClose= () => {
    this.setState({ isDeleteIndexerModalOpen: false });
  }

  onConfirmDeleteIndexer = () => {
    this.props.onConfirmDeleteIndexer(this.props.id);
  }

  onCloneIndexerPress = () => {
    const {
      id,
      onCloneIndexerPress
    } = this.props;

    onCloneIndexerPress(id);
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      enableRss,
      enableAutomaticSearch,
      enableInteractiveSearch,
      supportsRss,
      supportsSearch,
      priority,
      showPriority
    } = this.props;

    return (
      <Card
        className={styles.indexer}
        overlayContent={true}
        onPress={this.onEditIndexerPress}
      >
        <div className={styles.nameContainer}>
          <div className={styles.name}>
            {name}
          </div>

          <IconButton
            className={styles.cloneButton}
            title="Clone Indexer"
            name={icons.CLONE}
            onPress={this.onCloneIndexerPress}
          />
        </div>

        <div className={styles.enabled}>

          {
            supportsRss && enableRss &&
              <Label kind={kinds.SUCCESS}>
                RSS
              </Label>
          }

          {
            supportsSearch && enableAutomaticSearch &&
              <Label kind={kinds.SUCCESS}>
                Automatic Search
              </Label>
          }

          {
            supportsSearch && enableInteractiveSearch &&
              <Label kind={kinds.SUCCESS}>
                Interactive Search
              </Label>
          }

          {
            showPriority &&
              <Label kind={kinds.DEFAULT}>
                Priority: {priority}
              </Label>
          }
          {
            !enableRss && !enableAutomaticSearch && !enableInteractiveSearch &&
            <Label
              kind={kinds.DISABLED}
              outline={true}
            >
              Disabled
            </Label>
          }
        </div>

        <EditIndexerModalConnector
          id={id}
          isOpen={this.state.isEditIndexerModalOpen}
          onModalClose={this.onEditIndexerModalClose}
          onDeleteIndexerPress={this.onDeleteIndexerPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteIndexerModalOpen}
          kind={kinds.DANGER}
          title="Delete Indexer"
          message={`Are you sure you want to delete the indexer '${name}'?`}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDeleteIndexer}
          onCancel={this.onDeleteIndexerModalClose}
        />
      </Card>
    );
  }
}

Indexer.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  priority: PropTypes.number.isRequired,
  enableRss: PropTypes.bool.isRequired,
  enableAutomaticSearch: PropTypes.bool.isRequired,
  enableInteractiveSearch: PropTypes.bool.isRequired,
  supportsRss: PropTypes.bool.isRequired,
  supportsSearch: PropTypes.bool.isRequired,
  showPriority: PropTypes.bool.isRequired,
  onCloneIndexerPress: PropTypes.func.isRequired,
  onConfirmDeleteIndexer: PropTypes.func.isRequired
};

export default Indexer;
