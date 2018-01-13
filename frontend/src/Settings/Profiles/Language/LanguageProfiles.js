import PropTypes from 'prop-types';
import React, { Component } from 'react';
import sortByName from 'Utilities/Array/sortByName';
import { icons } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import LanguageProfile from './LanguageProfile';
import EditLanguageProfileModalConnector from './EditLanguageProfileModalConnector';
import styles from './LanguageProfiles.css';

class LanguageProfiles extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isLanguageProfileModalOpen: false
    };
  }

  //
  // Listeners

  onCloneLanguageProfilePress = (id) => {
    this.props.onCloneLanguageProfilePress(id);
    this.setState({ isLanguageProfileModalOpen: true });
  }

  onEditLanguageProfilePress = () => {
    this.setState({ isLanguageProfileModalOpen: true });
  }

  onModalClose = () => {
    this.setState({ isLanguageProfileModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      isDeleting,
      onConfirmDeleteLanguageProfile,
      onCloneLanguageProfilePress,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend="Language Profiles">
        <PageSectionContent
          errorMessage="Unable to load Language Profiles"
          {...otherProps}
        >
          <div className={styles.languageProfiles}>
            {
              items.sort(sortByName).map((item) => {
                return (
                  <LanguageProfile
                    key={item.id}
                    {...item}
                    isDeleting={isDeleting}
                    onConfirmDeleteLanguageProfile={onConfirmDeleteLanguageProfile}
                    onCloneLanguageProfilePress={this.onCloneLanguageProfilePress}
                  />
                );
              })
            }

            <Card
              className={styles.addLanguageProfile}
              onPress={this.onEditLanguageProfilePress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>
          </div>

          <EditLanguageProfileModalConnector
            isOpen={this.state.isLanguageProfileModalOpen}
            onModalClose={this.onModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

LanguageProfiles.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteLanguageProfile: PropTypes.func.isRequired,
  onCloneLanguageProfilePress: PropTypes.func.isRequired
};

export default LanguageProfiles;
