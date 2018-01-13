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
import styles from './EditRestrictionModalContent.css';

function EditRestrictionModalContent(props) {
  const {
    isSaving,
    saveError,
    item,
    onInputChange,
    onModalClose,
    onSavePress,
    onDeleteRestrictionPress,
    ...otherProps
  } = props;

  const {
    id,
    required,
    ignored,
    tags
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Restriction' : 'Add Restriction'}
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
            <FormLabel>Tags</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText="Restrictions will apply to series at least one matching tag. Leave blank to apply to all series"
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
              onPress={onDeleteRestrictionPress}
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

EditRestrictionModalContent.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onDeleteRestrictionPress: PropTypes.func
};

export default EditRestrictionModalContent;
