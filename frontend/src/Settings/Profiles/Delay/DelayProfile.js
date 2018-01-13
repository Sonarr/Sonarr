import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import titleCase from 'Utilities/String/titleCase';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import TagList from 'Components/TagList';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import EditDelayProfileModalConnector from './EditDelayProfileModalConnector';
import styles from './DelayProfile.css';

function getDelay(enabled, delay) {
  if (!enabled) {
    return '-';
  }

  if (!delay) {
    return 'No Delay';
  }

  if (delay === 1) {
    return '1 Minute';
  }

  // TODO: use better units of time than just minutes
  return `${delay} Minutes`;
}

class DelayProfile extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditDelayProfileModalOpen: false,
      isDeleteDelayProfileModalOpen: false
    };
  }

  //
  // Listeners

  onEditDelayProfilePress = () => {
    this.setState({ isEditDelayProfileModalOpen: true });
  }

  onEditDelayProfileModalClose = () => {
    this.setState({ isEditDelayProfileModalOpen: false });
  }

  onDeleteDelayProfilePress = () => {
    this.setState({
      isEditDelayProfileModalOpen: false,
      isDeleteDelayProfileModalOpen: true
    });
  }

  onDeleteDelayProfileModalClose = () => {
    this.setState({ isDeleteDelayProfileModalOpen: false });
  }

  onConfirmDeleteDelayProfile = () => {
    this.props.onConfirmDeleteDelayProfile(this.props.id);
  }

  //
  // Render

  render() {
    const {
      id,
      enableUsenet,
      enableTorrent,
      preferredProtocol,
      usenetDelay,
      torrentDelay,
      tags,
      tagList,
      isDragging,
      connectDragSource
    } = this.props;

    let preferred = titleCase(preferredProtocol);

    if (!enableUsenet) {
      preferred = 'Only Torrent';
    } else if (!enableTorrent) {
      preferred = 'Only Usenet';
    }

    return (
      <div
        className={classNames(
          styles.delayProfile,
          isDragging && styles.isDragging,
        )}
      >
        <div className={styles.column}>{preferred}</div>
        <div className={styles.column}>{getDelay(enableUsenet, usenetDelay)}</div>
        <div className={styles.column}>{getDelay(enableTorrent, torrentDelay)}</div>

        <TagList
          tags={tags}
          tagList={tagList}
        />

        <div className={styles.actions}>
          <Link
            className={id === 1 ? styles.editButton : undefined}
            onPress={this.onEditDelayProfilePress}
          >
            <Icon name={icons.EDIT} />
          </Link>

          {
            id !== 1 &&
              connectDragSource(
                <div className={styles.dragHandle}>
                  <Icon
                    className={styles.dragIcon}
                    name={icons.REORDER}
                  />
                </div>
              )
          }
        </div>

        <EditDelayProfileModalConnector
          id={id}
          isOpen={this.state.isEditDelayProfileModalOpen}
          onModalClose={this.onEditDelayProfileModalClose}
          onDeleteDelayProfilePress={this.onDeleteDelayProfilePress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteDelayProfileModalOpen}
          kind={kinds.DANGER}
          title="Delete Delay Profile"
          message="Are you sure you want to delete this delay profile?"
          confirmLabel="Delete"
          onConfirm={this.onConfirmDeleteDelayProfile}
          onCancel={this.onDeleteDelayProfileModalClose}
        />
      </div>
    );
  }
}

DelayProfile.propTypes = {
  id: PropTypes.number.isRequired,
  enableUsenet: PropTypes.bool.isRequired,
  enableTorrent: PropTypes.bool.isRequired,
  preferredProtocol: PropTypes.string.isRequired,
  usenetDelay: PropTypes.number.isRequired,
  torrentDelay: PropTypes.number.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDragging: PropTypes.bool.isRequired,
  connectDragSource: PropTypes.func,
  onConfirmDeleteDelayProfile: PropTypes.func.isRequired
};

DelayProfile.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default DelayProfile;
