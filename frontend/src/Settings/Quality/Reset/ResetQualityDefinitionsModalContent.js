import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ResetQualityDefinitionsModalContent.css';

class ResetQualityDefinitionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      resetDefinitionTitles: false
    };
  }

  //
  // Listeners

  onResetDefinitionTitlesChange = ({ value }) => {
    this.setState({ resetDefinitionTitles: value });
  };

  onResetQualityDefinitionsConfirmed = () => {
    const resetDefinitionTitles = this.state.resetDefinitionTitles;

    this.setState({ resetDefinitionTitles: false });
    this.props.onResetQualityDefinitions(resetDefinitionTitles);
  };

  //
  // Render

  render() {
    const {
      onModalClose,
      isResettingQualityDefinitions
    } = this.props;

    const resetDefinitionTitles = this.state.resetDefinitionTitles;

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('ResetQualityDefinitions')}
        </ModalHeader>

        <ModalBody>
          <div className={styles.messageContainer}>
            {translate('ResetQualityDefinitionsMessageText')}
          </div>

          <FormGroup>
            <FormLabel>
              {translate('ResetTitles')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="resetDefinitionTitles"
              value={resetDefinitionTitles}
              helpText={translate('ResetDefinitionTitlesHelpText')}
              onChange={this.onResetDefinitionTitlesChange}
            />
          </FormGroup>

        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Cancel')}
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={this.onResetQualityDefinitionsConfirmed}
            isDisabled={isResettingQualityDefinitions}
          >
            {translate('Reset')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

ResetQualityDefinitionsModalContent.propTypes = {
  onResetQualityDefinitions: PropTypes.func.isRequired,
  isResettingQualityDefinitions: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ResetQualityDefinitionsModalContent;
