import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import EditLanguageProfileModalConnector from './EditLanguageProfileModalConnector';
import styles from './LanguageProfile.css';

class LanguageProfile extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditLanguageProfileModalOpen: false,
      isDeleteLanguageProfileModalOpen: false
    };
  }

  //
  // Listeners

  onEditLanguageProfilePress = () => {
    this.setState({ isEditLanguageProfileModalOpen: true });
  };

  onEditLanguageProfileModalClose = () => {
    this.setState({ isEditLanguageProfileModalOpen: false });
  };

  onDeleteLanguageProfilePress = () => {
    this.setState({
      isEditLanguageProfileModalOpen: false,
      isDeleteLanguageProfileModalOpen: true
    });
  };

  onDeleteLanguageProfileModalClose = () => {
    this.setState({ isDeleteLanguageProfileModalOpen: false });
  };

  onConfirmDeleteLanguageProfile = () => {
    this.props.onConfirmDeleteLanguageProfile(this.props.id);
  };

  onCloneLanguageProfilePress = () => {
    const {
      id,
      onCloneLanguageProfilePress
    } = this.props;

    onCloneLanguageProfilePress(id);
  };

  //
  // Render

  render() {
    const {
      id,
      name,
      upgradeAllowed,
      cutoff,
      languages,
      isDeleting
    } = this.props;

    return (
      <Card
        className={styles.languageProfile}
        overlayContent={true}
        onPress={this.onEditLanguageProfilePress}
      >
        <div className={styles.nameContainer}>
          <div className={styles.name}>
            {name}
          </div>

          <IconButton
            className={styles.cloneButton}
            title="Clone Profile"
            name={icons.CLONE}
            onPress={this.onCloneLanguageProfilePress}
          />
        </div>

        <div className={styles.languages}>
          {
            languages.map((item) => {
              if (!item.allowed) {
                return null;
              }

              const isCutoff = upgradeAllowed && item.language.id === cutoff.id;

              return (
                <Label
                  key={item.language.id}
                  kind={isCutoff ? kinds.INFO : kinds.DEFAULT}
                  title={isCutoff ? 'Upgrade until this language is met or exceeded' : null}
                >
                  {item.language.name}
                </Label>
              );
            })
          }
        </div>

        <EditLanguageProfileModalConnector
          id={id}
          isOpen={this.state.isEditLanguageProfileModalOpen}
          onModalClose={this.onEditLanguageProfileModalClose}
          onDeleteLanguageProfilePress={this.onDeleteLanguageProfilePress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteLanguageProfileModalOpen}
          kind={kinds.DANGER}
          title="Delete Language Profile"
          message={`Are you sure you want to delete the language profile '${name}'?`}
          confirmLabel="Delete"
          isSpinning={isDeleting}
          onConfirm={this.onConfirmDeleteLanguageProfile}
          onCancel={this.onDeleteLanguageProfileModalClose}
        />
      </Card>
    );
  }
}

LanguageProfile.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  upgradeAllowed: PropTypes.bool.isRequired,
  cutoff: PropTypes.object.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteLanguageProfile: PropTypes.func.isRequired,
  onCloneLanguageProfilePress: PropTypes.func.isRequired
};

export default LanguageProfile;
