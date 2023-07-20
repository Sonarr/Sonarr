import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import ImportCustomFormatModal from './ImportCustomFormatModal';
import AddSpecificationModal from './Specifications/AddSpecificationModal';
import EditSpecificationModalConnector from './Specifications/EditSpecificationModalConnector';
import Specification from './Specifications/Specification';
import styles from './EditCustomFormatModalContent.css';

class EditCustomFormatModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddSpecificationModalOpen: false,
      isEditSpecificationModalOpen: false,
      isImportCustomFormatModalOpen: false
    };
  }

  //
  // Listeners

  onAddSpecificationPress = () => {
    this.setState({ isAddSpecificationModalOpen: true });
  };

  onAddSpecificationModalClose = ({ specificationSelected = false } = {}) => {
    this.setState({
      isAddSpecificationModalOpen: false,
      isEditSpecificationModalOpen: specificationSelected
    });
  };

  onEditSpecificationModalClose = () => {
    this.setState({ isEditSpecificationModalOpen: false });
  };

  onImportPress = () => {
    this.setState({ isImportCustomFormatModalOpen: true });
  };

  onImportCustomFormatModalClose = () => {
    this.setState({ isImportCustomFormatModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      isSaving,
      saveError,
      item,
      specificationsPopulated,
      specifications,
      onInputChange,
      onSavePress,
      onModalClose,
      onDeleteCustomFormatPress,
      onCloneSpecificationPress,
      onConfirmDeleteSpecification,
      ...otherProps
    } = this.props;

    const {
      isAddSpecificationModalOpen,
      isEditSpecificationModalOpen,
      isImportCustomFormatModalOpen
    } = this.state;

    const {
      id,
      name,
      includeCustomFormatWhenRenaming
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>

        <ModalHeader>
          {id ? translate('EditCustomFormat') : translate('AddCustomFormat')}
        </ModalHeader>

        <ModalBody>
          <div>
            {
              isFetching &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <div>
                  {translate('UnableToAddANewCustomFormatPleaseTryAgain')}
                </div>
            }

            {
              !isFetching && !error && specificationsPopulated &&
                <div>
                  <Form
                    {...otherProps}
                  >
                    <FormGroup>
                      <FormLabel>
                        {translate('Name')}
                      </FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="name"
                        {...name}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>{translate('IncludeCustomFormatWhenRenaming')}</FormLabel>

                      <FormInputGroup
                        type={inputTypes.CHECK}
                        name="includeCustomFormatWhenRenaming"
                        helpText={translate('IncludeCustomFormatWhenRenamingHelpText')}
                        {...includeCustomFormatWhenRenaming}
                        onChange={onInputChange}
                      />
                    </FormGroup>
                  </Form>

                  <FieldSet legend={translate('Conditions')}>
                    <div className={styles.customFormats}>
                      {
                        specifications.map((tag) => {
                          return (
                            <Specification
                              key={tag.id}
                              {...tag}
                              onCloneSpecificationPress={onCloneSpecificationPress}
                              onConfirmDeleteSpecification={onConfirmDeleteSpecification}
                            />
                          );
                        })
                      }

                      <Card
                        className={styles.addSpecification}
                        onPress={this.onAddSpecificationPress}
                      >
                        <div className={styles.center}>
                          <Icon
                            name={icons.ADD}
                            size={45}
                          />
                        </div>
                      </Card>
                    </div>
                  </FieldSet>

                  <AddSpecificationModal
                    isOpen={isAddSpecificationModalOpen}
                    onModalClose={this.onAddSpecificationModalClose}
                  />

                  <EditSpecificationModalConnector
                    isOpen={isEditSpecificationModalOpen}
                    onModalClose={this.onEditSpecificationModalClose}
                  />

                  <ImportCustomFormatModal
                    isOpen={isImportCustomFormatModalOpen}
                    onModalClose={this.onImportCustomFormatModalClose}
                  />

                </div>
            }
          </div>
        </ModalBody>
        <ModalFooter>
          <div className={styles.rightButtons}>
            {
              id &&
                <Button
                  className={styles.deleteButton}
                  kind={kinds.DANGER}
                  onPress={onDeleteCustomFormatPress}
                >
                  {translate('Delete')}
                </Button>
            }

            <Button
              className={styles.deleteButton}
              onPress={this.onImportPress}
            >
              {translate('Import')}
            </Button>
          </div>

          <Button
            onPress={onModalClose}
          >
            {translate('Cancel')}
          </Button>

          <SpinnerErrorButton
            isSpinning={isSaving}
            error={saveError}
            onPress={onSavePress}
          >
            {translate('Save')}
          </SpinnerErrorButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

EditCustomFormatModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  specificationsPopulated: PropTypes.bool.isRequired,
  specifications: PropTypes.arrayOf(PropTypes.object),
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onContentHeightChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteCustomFormatPress: PropTypes.func,
  onCloneSpecificationPress: PropTypes.func.isRequired,
  onConfirmDeleteSpecification: PropTypes.func.isRequired
};

export default EditCustomFormatModalContent;
