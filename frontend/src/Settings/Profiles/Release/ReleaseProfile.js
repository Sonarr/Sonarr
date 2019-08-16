import PropTypes from 'prop-types';
import React, { Component } from 'react';
import split from 'Utilities/String/split';
import { kinds } from 'Helpers/Props';
import Card from 'Components/Card';
import Label from 'Components/Label';
import TagList from 'Components/TagList';
import ConfirmModal from 'Components/Modal/ConfirmModal';
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
  }

  onEditReleaseProfileModalClose = () => {
    this.setState({ isEditReleaseProfileModalOpen: false });
  }

  onDeleteReleaseProfilePress = () => {
    this.setState({
      isEditReleaseProfileModalOpen: false,
      isDeleteReleaseProfileModalOpen: true
    });
  }

  onDeleteReleaseProfileModalClose= () => {
    this.setState({ isDeleteReleaseProfileModalOpen: false });
  }

  onConfirmDeleteReleaseProfile = () => {
    this.props.onConfirmDeleteReleaseProfile(this.props.id);
  }

  //
  // Render

  render() {
    const {
      id,
      required,
      ignored,
      preferred,
      tags,
      tagList
    } = this.props;

    const {
      isEditReleaseProfileModalOpen,
      isDeleteReleaseProfileModalOpen
    } = this.state;

    return (
      <Card
        className={styles.releaseProfile}
        overlayContent={true}
        onPress={this.onEditReleaseProfilePress}
      >
        <div>
          {
            split(required).map((item) => {
              if (!item) {
                return null;
              }

              return (
                <Label
                  key={item}
                  kind={kinds.SUCCESS}
                >
                  {item}
                </Label>
              );
            })
          }
        </div>

        <div>
          {
            split(ignored).map((item) => {
              if (!item) {
                return null;
              }

              return (
                <Label
                  key={item}
                  kind={kinds.DANGER}
                >
                  {item}
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
                  key={item.key}
                  kind={isPreferred ? kinds.DEFAULT : kinds.WARNING}
                >
                  {item.key} {isPreferred && '+'}{item.value}
                </Label>
              );
            })
          }
        </div>

        <TagList
          tags={tags}
          tagList={tagList}
        />

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
  required: PropTypes.string.isRequired,
  ignored: PropTypes.string.isRequired,
  preferred: PropTypes.arrayOf(PropTypes.object).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteReleaseProfile: PropTypes.func.isRequired
};

ReleaseProfile.defaultProps = {
  required: '',
  ignored: '',
  preferred: []
};

export default ReleaseProfile;
