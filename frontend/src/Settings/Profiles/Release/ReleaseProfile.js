import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MiddleTruncate from 'react-middle-truncate';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { kinds } from 'Helpers/Props';
import EditReleaseProfileModalConnector from './EditReleaseProfileModalConnector';
import styles from './ReleaseProfile.css';

class ReleaseProfile extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditReleaseProfileModalOpen: false,
      isDeleteReleaseProfileModalOpen: false
    };
  }

  //
  // Listeners

  onEditReleaseProfilePress = () => {
    this.setState({ isEditReleaseProfileModalOpen: true });
  };

  onEditReleaseProfileModalClose = () => {
    this.setState({ isEditReleaseProfileModalOpen: false });
  };

  onDeleteReleaseProfilePress = () => {
    this.setState({
      isEditReleaseProfileModalOpen: false,
      isDeleteReleaseProfileModalOpen: true
    });
  };

  onDeleteReleaseProfileModalClose= () => {
    this.setState({ isDeleteReleaseProfileModalOpen: false });
  };

  onConfirmDeleteReleaseProfile = () => {
    this.props.onConfirmDeleteReleaseProfile(this.props.id);
  };

  //
  // Render

  render() {
    const {
      id,
      name,
      enabled,
      required,
      ignored,
      preferred,
      tags,
      indexerId,
      tagList,
      indexerList
    } = this.props;

    const {
      isEditReleaseProfileModalOpen,
      isDeleteReleaseProfileModalOpen
    } = this.state;

    const indexer = indexerId !== 0 && _.find(indexerList, { id: indexerId });

    return (
      <Card
        className={styles.releaseProfile}
        overlayContent={true}
        onPress={this.onEditReleaseProfilePress}
      >
        {
          name ?
            <div className={styles.name}>
              {name}
            </div> :
            null
        }

        <div>
          {
            required.map((item) => {
              if (!item) {
                return null;
              }

              return (
                <Label
                  className={styles.label}
                  key={item}
                  kind={kinds.SUCCESS}
                >
                  <MiddleTruncate
                    text={item}
                    start={10}
                    end={10}
                  />
                </Label>
              );
            })
          }
        </div>

        <div>
          {
            preferred.map((item) => {
              const isPreferred = item.value >= 0;

              return (
                <Label
                  className={styles.label}
                  key={item.key}
                  kind={isPreferred ? kinds.DEFAULT : kinds.WARNING}
                >
                  <MiddleTruncate
                    text={`${item.key} ${isPreferred ? '+' : ''}${item.value}`}
                    start={10}
                    end={14}
                  />
                </Label>
              );
            })
          }
        </div>

        <div>
          {
            ignored.map((item) => {
              if (!item) {
                return null;
              }

              return (
                <Label
                  className={styles.label}
                  key={item}
                  kind={kinds.DANGER}
                >
                  <MiddleTruncate
                    text={item}
                    start={10}
                    end={10}
                  />
                </Label>
              );
            })
          }
        </div>

        <TagList
          tags={tags}
          tagList={tagList}
        />

        <div>
          {
            !enabled &&
              <Label
                kind={kinds.DISABLED}
                outline={true}
              >
                Disabled
              </Label>
          }

          {
            indexer &&
              <Label
                kind={kinds.INFO}
                outline={true}
              >
                {indexer.name}
              </Label>
          }
        </div>

        <EditReleaseProfileModalConnector
          id={id}
          isOpen={isEditReleaseProfileModalOpen}
          onModalClose={this.onEditReleaseProfileModalClose}
          onDeleteReleaseProfilePress={this.onDeleteReleaseProfilePress}
        />

        <ConfirmModal
          isOpen={isDeleteReleaseProfileModalOpen}
          kind={kinds.DANGER}
          title="Delete ReleaseProfile"
          message={'Are you sure you want to delete this releaseProfile?'}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDeleteReleaseProfile}
          onCancel={this.onDeleteReleaseProfileModalClose}
        />
      </Card>
    );
  }
}

ReleaseProfile.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string,
  enabled: PropTypes.bool.isRequired,
  required: PropTypes.arrayOf(PropTypes.string).isRequired,
  ignored: PropTypes.arrayOf(PropTypes.string).isRequired,
  preferred: PropTypes.arrayOf(PropTypes.object).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  indexerId: PropTypes.number.isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  indexerList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteReleaseProfile: PropTypes.func.isRequired
};

ReleaseProfile.defaultProps = {
  enabled: true,
  required: [],
  ignored: [],
  preferred: [],
  indexerId: 0
};

export default ReleaseProfile;
