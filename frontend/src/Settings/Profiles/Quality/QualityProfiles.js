import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import EditQualityProfileModalConnector from './EditQualityProfileModalConnector';
import QualityProfile from './QualityProfile';
import styles from './QualityProfiles.css';

class QualityProfiles extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isQualityProfileModalOpen: false
    };
  }

  //
  // Listeners

  onCloneQualityProfilePress = (id) => {
    this.props.onCloneQualityProfilePress(id);
    this.setState({ isQualityProfileModalOpen: true });
  };

  onEditQualityProfilePress = () => {
    this.setState({ isQualityProfileModalOpen: true });
  };

  onModalClose = () => {
    this.setState({ isQualityProfileModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      isDeleting,
      onConfirmDeleteQualityProfile,
      onCloneQualityProfilePress,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend="Quality Profiles">
        <PageSectionContent
          errorMessage="Unable to load Quality Profiles"
          {...otherProps}c={true}
        >
          <div className={styles.qualityProfiles}>
            {
              items.map((item) => {
                return (
                  <QualityProfile
                    key={item.id}
                    {...item}
                    isDeleting={isDeleting}
                    onConfirmDeleteQualityProfile={onConfirmDeleteQualityProfile}
                    onCloneQualityProfilePress={this.onCloneQualityProfilePress}
                  />
                );
              })
            }

            <Card
              className={styles.addQualityProfile}
              onPress={this.onEditQualityProfilePress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>
          </div>

          <EditQualityProfileModalConnector
            isOpen={this.state.isQualityProfileModalOpen}
            onModalClose={this.onModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

QualityProfiles.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteQualityProfile: PropTypes.func.isRequired,
  onCloneQualityProfilePress: PropTypes.func.isRequired
};

export default QualityProfiles;
