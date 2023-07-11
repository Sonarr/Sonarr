import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import TagDetailsModal from './Details/TagDetailsModal';
import TagInUse from './TagInUse';
import styles from './Tag.css';

class Tag extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false,
      isDeleteTagModalOpen: false
    };
  }

  //
  // Listeners

  onShowDetailsPress = () => {
    this.setState({ isDetailsModalOpen: true });
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  };

  onDeleteTagPress = () => {
    this.setState({
      isDetailsModalOpen: false,
      isDeleteTagModalOpen: true
    });
  };

  onDeleteTagModalClose= () => {
    this.setState({ isDeleteTagModalOpen: false });
  };

  onConfirmDeleteTag = () => {
    this.props.onConfirmDeleteTag({ id: this.props.id });
  };

  //
  // Render

  render() {
    const {
      label,
      delayProfileIds,
      importListIds,
      notificationIds,
      restrictionIds,
      indexerIds,
      downloadClientIds,
      autoTagIds,
      seriesIds
    } = this.props;

    const {
      isDetailsModalOpen,
      isDeleteTagModalOpen
    } = this.state;

    const isTagUsed = !!(
      delayProfileIds.length ||
      importListIds.length ||
      notificationIds.length ||
      restrictionIds.length ||
      indexerIds.length ||
      downloadClientIds.length ||
      autoTagIds.length ||
      seriesIds.length
    );

    return (
      <Card
        className={styles.tag}
        overlayContent={true}
        onPress={this.onShowDetailsPress}
      >
        <div className={styles.label}>
          {label}
        </div>

        {
          isTagUsed ?
            <div>
              <TagInUse
                label="series"
                count={seriesIds.length}
                shouldPluralize={false}
              />

              <TagInUse
                label="delay profile"
                count={delayProfileIds.length}
              />

              <TagInUse
                label="import list"
                count={importListIds.length}
              />

              <TagInUse
                label="connection"
                count={notificationIds.length}
              />

              <TagInUse
                label="release profile"
                count={restrictionIds.length}
              />

              <TagInUse
                label="indexer"
                count={indexerIds.length}
              />

              <TagInUse
                label="download client"
                count={downloadClientIds.length}
              />

              <TagInUse
                label="auto tagging"
                count={autoTagIds.length}
                shouldPluralize={false}
              />
            </div> :
            null
        }

        {
          !isTagUsed &&
            <div>
              No links
            </div>
        }

        <TagDetailsModal
          label={label}
          isTagUsed={isTagUsed}
          seriesIds={seriesIds}
          delayProfileIds={delayProfileIds}
          importListIds={importListIds}
          notificationIds={notificationIds}
          restrictionIds={restrictionIds}
          indexerIds={indexerIds}
          downloadClientIds={downloadClientIds}
          autoTagIds={autoTagIds}
          isOpen={isDetailsModalOpen}
          onModalClose={this.onDetailsModalClose}
          onDeleteTagPress={this.onDeleteTagPress}
        />

        <ConfirmModal
          isOpen={isDeleteTagModalOpen}
          kind={kinds.DANGER}
          title="Delete Tag"
          message={`Are you sure you want to delete the tag '${label}'?`}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDeleteTag}
          onCancel={this.onDeleteTagModalClose}
        />
      </Card>
    );
  }
}

Tag.propTypes = {
  id: PropTypes.number.isRequired,
  label: PropTypes.string.isRequired,
  delayProfileIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  importListIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  notificationIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  restrictionIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  indexerIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  downloadClientIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  autoTagIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  seriesIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  onConfirmDeleteTag: PropTypes.func.isRequired
};

Tag.defaultProps = {
  delayProfileIds: [],
  importListIds: [],
  notificationIds: [],
  restrictionIds: [],
  indexerIds: [],
  downloadClientIds: [],
  autoTagIds: [],
  seriesIds: []
};

export default Tag;
