import PropTypes from 'prop-types';
import React from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import styles from './EditReleaseProfileModalContent.css';

const tagInputDelimiters = ['Tab', 'Enter'];

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
    name,
    enabled,
    required,
    ignored,
    preferred,
    includePreferredWhenRenaming,
    tags,
    indexerId
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Release Profile' : 'Add Release Profile'}
      </ModalHeader>

      <ModalBody>
        <Form {...otherProps}>

          <FormGroup>
            <FormLabel>Name</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              placeholder="Optional name"
              canEdit={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Enable Profile</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enabled"
              helpText="Check to enable release profile"
              {...enabled}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Must Contain</FormLabel>

            <FormInputGroup
              {...required}
              type={inputTypes.TEXT_TAG}
              name="required"
              helpText="The release must contain at least one of these terms (case insensitive)"
              kind={kinds.SUCCESS}
              placeholder="Add new restriction"
              delimiters={tagInputDelimiters}
              canEdit={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Must Not Contain</FormLabel>

            <FormInputGroup
              {...ignored}
              type={inputTypes.TEXT_TAG}
              name="ignored"
              helpText="The release will be rejected if it contains one or more of terms (case insensitive)"
              kind={kinds.DANGER}
              placeholder="Add new restriction"
              delimiters={tagInputDelimiters}
              canEdit={true}
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
              helpText={indexerId.value === 0 ? 'Include in {Preferred Words} renaming format' : 'Only supported when Indexer is set to (All)'}
              {...includePreferredWhenRenaming}
              onChange={onInputChange}
              isDisabled={indexerId.value !== 0}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Indexer</FormLabel>

            <FormInputGroup
              type={inputTypes.INDEXER_SELECT}
              name="indexerId"
              helpText="Specify what indexer the profile applies to"
              helpTextWarning="Using a specific indexer with preferred words can lead to duplicate releases being grabbed"
              {...indexerId}
              includeAny={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Tags</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText="Release profiles will apply to series with at least one matching tag. Leave blank to apply to all series"
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
