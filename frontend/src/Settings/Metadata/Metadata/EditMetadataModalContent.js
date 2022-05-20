import PropTypes from 'prop-types';
import React from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';

function EditMetadataModalContent(props) {
  const {
    advancedSettings,
    isSaving,
    saveError,
    item,
    onInputChange,
    onFieldChange,
    onModalClose,
    onSavePress,
    ...otherProps
  } = props;

  const {
    name,
    enable,
    fields
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Edit {name.value} Metadata
      </ModalHeader>

      <ModalBody>
        <Form {...otherProps}>
          <FormGroup>
            <FormLabel>Enable</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enable"
              helpText="Enable metadata file creation for this metadata type"
              {...enable}
              onChange={onInputChange}
            />
          </FormGroup>

          {
            fields.map((field) => {
              return (
                <ProviderFieldFormGroup
                  key={field.name}
                  advancedSettings={advancedSettings}
                  provider="metadata"
                  {...field}
                  isDisabled={!enable.value}
                  onChange={onFieldChange}
                />
              );
            })
          }

        </Form>
      </ModalBody>

      <ModalFooter>
        <Button
          onPress={onModalClose}
        >
          Cancel
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          Save
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditMetadataModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onDeleteMetadataPress: PropTypes.func
};

export default EditMetadataModalContent;
