import React, { useCallback, useEffect } from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
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

  const {
    name,
    enabled,
    required,
    ignored,
    airDateRestriction,
    airDateGracePeriod,
    allowSeasonPackWithoutAllEpisodesAired,
    indexerIds,
    tags,
    excludedTags,
  } = item;

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
          <FormRow>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInput
              type={inputTypes.TEXT}
              name="name"
              {...name}
              placeholder={translate('OptionalName')}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('EnableProfile')}</FormLabel>
            <FormInputHelpText text={translate('EnableProfileHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="enabled"
              {...enabled}
              onChange={handleInputChange}
            />
          </FormRow>

          <ModalSection title={translate('ProfileSectionPatterns')}>
            <FormRow>
              <FormLabel>{translate('MustContain')}</FormLabel>
              <FormInputHelpText text={translate('MustContainHelpText')} />
              <FormInput
                {...required}
                inputClassName={styles.tagInternalInput}
                type={inputTypes.TEXT_TAG}
                name="required"
                kind={kinds.SUCCESS}
                placeholder={translate('AddNewRestriction')}
                delimiters={tagInputDelimiters}
                canEdit={true}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('MustNotContain')}</FormLabel>
              <FormInputHelpText text={translate('MustNotContainHelpText')} />
              <FormInput
                {...ignored}
                inputClassName={styles.tagInternalInput}
                type={inputTypes.TEXT_TAG}
                name="ignored"
                kind={kinds.DANGER}
                placeholder={translate('AddNewRestriction')}
                delimiters={tagInputDelimiters}
                canEdit={true}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('AirDateRestriction')}</FormLabel>
              <FormInputHelpText
                text={translate('AirDateRestrictionHelpText')}
              />
              <FormInput
                {...airDateRestriction}
                type={inputTypes.CHECK}
                name="airDateRestriction"
                onChange={handleInputChange}
              />
            </FormRow>

            {airDateRestriction.value ? (
              <FormRow>
                <FormLabel>{translate('AirDateGracePeriod')}</FormLabel>
                <FormInputHelpText
                  text={translate('AirDateGracePeriodHelpText')}
                />
                <FormInput
                  {...airDateGracePeriod}
                  type={inputTypes.NUMBER}
                  unit="days"
                  name="airDateGracePeriod"
                  onChange={handleInputChange}
                />
              </FormRow>
            ) : null}

            <FormRow>
              <FormLabel>
                {translate('AllowSeasonPackWithoutAllEpisodesAired')}
              </FormLabel>
              <FormInputHelpText
                text={translate(
                  'AllowSeasonPackWithoutAllEpisodesAiredHelpText'
                )}
              />
              <FormInputHelpText
                text={translate(
                  'AllowSeasonPackWithoutAllEpisodesAiredHelpTextWarning'
                )}
                isWarning={true}
              />
              <FormInput
                {...allowSeasonPackWithoutAllEpisodesAired}
                type={inputTypes.CHECK}
                name="allowSeasonPackWithoutAllEpisodesAired"
                onChange={handleInputChange}
              />
            </FormRow>
          </ModalSection>

          <ModalSection title={translate('ProfileSectionScope')}>
            <FormRow>
              <FormLabel>{translate('Indexer')}</FormLabel>
              <FormInputHelpText
                text={translate('ReleaseProfileIndexerHelpText')}
              />
              <FormInputHelpText
                text={translate('ReleaseProfileIndexerHelpTextWarning')}
                isWarning={true}
              />
              <FormInput
                type={inputTypes.INDEXER_SELECT}
                name="indexerIds"
                {...indexerIds}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('Tags')}</FormLabel>
              <FormInputHelpText
                text={translate('ReleaseProfileTagSeriesHelpText')}
              />
              <FormInput
                type={inputTypes.TAG}
                name="tags"
                {...tags}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('ExcludedTags')}</FormLabel>
              <FormInputHelpText
                text={translate('ReleaseProfileExcludedTagSeriesHelpText')}
              />
              <FormInput
                type={inputTypes.TAG}
                name="excludedTags"
                kind={kinds.DANGER}
                {...excludedTags}
                onChange={handleInputChange}
              />
            </FormRow>
          </ModalSection>
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
