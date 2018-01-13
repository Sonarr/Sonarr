import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import styles from './EditReleaseProfileModalContent.css';

function EditReleaseProfileModalContent(props) {
  const {
    isSaving,
    saveError,
    item,
    onInputChange,
    onModalClose,
    onSavePress,
    onDeleteReleaseProfilePress,
    ...otherProps
  } = props;

  const {
    id,
    required,
    ignored,
    preferred,
    includePreferredWhenRenaming,
    tags
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Release Profile' : 'Add Release Profile'}
      </ModalHeader>

      <ModalBody>
        <Form
          {...otherProps}
        >
          <FormGroup>
            <FormLabel>Must Contain</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT_TAG}
              name="required"
              helpText="The release must contain at least one of these terms (case insensitive)"
              kind={kinds.SUCCESS}
              placeholder="Add new restriction"
              {...required}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Must Not Contain</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT_TAG}
              name="ignored"
              helpText="The release will be rejected if it contains one or more of terms (case insensitive)"
              kind={kinds.DANGER}
              placeholder="Add new restriction"
              {...ignored}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Preferred</FormLabel>

            <FormInputGroup
              type={inputTypes.KEY_VALUE_LIST}
              name="preferred"
              helpTexts={[
                'The release will be preferred based on the each term\'s score (case insensitive)',
                'A positive score will be more preferred',
                'A negative score will be less preferred'
              ]}
              {...preferred}
              keyPlaceholder="Term"
              valuePlaceholder="Score"
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Include Preferred when Renaming</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="includePreferredWhenRenaming"
              helpText="If Preferred Words is included in the naming format include the preferred words above"
              {...includePreferredWhenRenaming}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Tags</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText="Release profiles will apply to series at least one matching tag. Leave blank to apply to all series"
              {...tags}
              onChange={onInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteReleaseProfilePress}
            >
              Delete
            </Button>
        }

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

EditReleaseProfileModalContent.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onDeleteReleaseProfilePress: PropTypes.func
};

export default EditReleaseProfileModalContent;
