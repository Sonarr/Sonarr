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
import translate from 'Utilities/String/translate';
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
    tags,
    indexerId
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditReleaseProfile') : translate('AddReleaseProfile')}
      </ModalHeader>

      <ModalBody>
        <Form {...otherProps}>

          <FormGroup>
            <FormLabel>Name</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              placeholder={translate('OptionalName')}
              canEdit={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('EnableProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enabled"
              helpText={translate('EnableProfileHelpText')}
              {...enabled}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MustContain')}</FormLabel>

            <FormInputGroup
              {...required}
              inputClassName={styles.tagInternalInput}
              type={inputTypes.TEXT_TAG}
              name="required"
              helpText={translate('MustContainHelpText')}
              kind={kinds.SUCCESS}
              placeholder={translate('AddNewRestriction')}
              delimiters={tagInputDelimiters}
              canEdit={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MustNotContain')}</FormLabel>

            <FormInputGroup
              {...ignored}
              inputClassName={styles.tagInternalInput}
              type={inputTypes.TEXT_TAG}
              name="ignored"
              helpText={translate('MustNotContainHelpText')}
              kind={kinds.DANGER}
              placeholder={translate('AddNewRestriction')}
              delimiters={tagInputDelimiters}
              canEdit={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Indexer')}</FormLabel>

            <FormInputGroup
              type={inputTypes.INDEXER_SELECT}
              name="indexerId"
              helpText={translate('ReleaseProfileIndexerHelpText')}
              helpTextWarning={translate('ReleaseProfileIndexerHelpTextWarning')}
              {...indexerId}
              includeAny={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText={translate('ReleaseProfileTagSeriesHelpText')}
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
              {translate('Delete')}
            </Button>
        }

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
