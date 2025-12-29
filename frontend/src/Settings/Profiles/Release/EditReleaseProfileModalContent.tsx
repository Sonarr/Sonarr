import React, { useCallback, useEffect } from 'react';
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
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useManageReleaseProfile } from './useReleaseProfiles';
import styles from './EditReleaseProfileModalContent.css';

const tagInputDelimiters = ['Tab', 'Enter'];

interface EditReleaseProfileModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteReleaseProfilePress?: () => void;
}

function EditReleaseProfileModalContent({
  id,
  onModalClose,
  onDeleteReleaseProfilePress,
}: EditReleaseProfileModalContentProps) {
  const {
    item,
    isSaving,
    saveError,
    validationErrors,
    validationWarnings,
    updateValue,
    saveProvider,
  } = useManageReleaseProfile(id ?? 0);

  const { name, enabled, required, ignored, tags, excludedTags, indexerId } =
    item;

  const wasSaving = usePrevious(isSaving);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - change is not yet typed
      updateValue(change.name, change.value);
    },
    [updateValue]
  );

  const handleSavePress = useCallback(() => {
    saveProvider();
  }, [saveProvider]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditReleaseProfile') : translate('AddReleaseProfile')}
      </ModalHeader>

      <ModalBody>
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          <FormGroup>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              placeholder={translate('OptionalName')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('EnableProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enabled"
              helpText={translate('EnableProfileHelpText')}
              {...enabled}
              onChange={handleInputChange}
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
              onChange={handleInputChange}
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
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Indexer')}</FormLabel>

            <FormInputGroup
              type={inputTypes.INDEXER_SELECT}
              name="indexerId"
              helpText={translate('ReleaseProfileIndexerHelpText')}
              helpTextWarning={translate(
                'ReleaseProfileIndexerHelpTextWarning'
              )}
              {...indexerId}
              includeAny={true}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText={translate('ReleaseProfileTagSeriesHelpText')}
              {...tags}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ExcludedTags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="excludedTags"
              helpText={translate('ReleaseProfileExcludedTagSeriesHelpText')}
              kind={kinds.DANGER}
              {...excludedTags}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteReleaseProfilePress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditReleaseProfileModalContent;
