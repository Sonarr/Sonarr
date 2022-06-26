import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import EditReleaseProfileModalConnector from './EditReleaseProfileModalConnector';
import ReleaseProfile from './ReleaseProfile';
import styles from './ReleaseProfiles.css';

class ReleaseProfiles extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddReleaseProfileModalOpen: false
    };
  }

  //
  // Listeners

  onAddReleaseProfilePress = () => {
    this.setState({ isAddReleaseProfileModalOpen: true });
  };

  onAddReleaseProfileModalClose = () => {
    this.setState({ isAddReleaseProfileModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      tagList,
      indexerList,
      onConfirmDeleteReleaseProfile,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend="Release Profiles">
        <PageSectionContent
          errorMessage="Unable to load ReleaseProfiles"
          {...otherProps}
        >
          <div className={styles.releaseProfiles}>
            <Card
              className={styles.addReleaseProfile}
              onPress={this.onAddReleaseProfilePress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>

            {
              items.map((item) => {
                return (
                  <ReleaseProfile
                    key={item.id}
                    tagList={tagList}
                    indexerList={indexerList}
                    {...item}
                    onConfirmDeleteReleaseProfile={onConfirmDeleteReleaseProfile}
                  />
                );
              })
            }
          </div>

          <EditReleaseProfileModalConnector
            isOpen={this.state.isAddReleaseProfileModalOpen}
            onModalClose={this.onAddReleaseProfileModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

ReleaseProfiles.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  indexerList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteReleaseProfile: PropTypes.func.isRequired
};

export default ReleaseProfiles;
