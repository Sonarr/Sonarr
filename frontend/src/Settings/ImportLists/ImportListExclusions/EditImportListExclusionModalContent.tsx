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
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useManageImportListExclusion } from './useImportListExclusions';
import styles from './EditImportListExclusionModalContent.css';

interface EditImportListExclusionModalContentProps {
  id?: number;
  title?: string;
  tvdbId?: number;
  onModalClose: () => void;
  onDeleteImportListExclusionPress?: () => void;
}

function EditImportListExclusionModalContent({
  id,
  title: existingTitle,
  tvdbId: existingTvdbId,
  onModalClose,
  onDeleteImportListExclusionPress,
}: EditImportListExclusionModalContentProps) {
  const {
    item,
    isSaving,
    saveError,
    validationErrors,
    validationWarnings,
    updateValue,
    save,
  } = useManageImportListExclusion({
    id,
    title: existingTitle,
    tvdbId: existingTvdbId,
  });

  const { title, tvdbId } = item;
  const wasSaving = usePrevious(isSaving);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      updateValue(name, value);
    },
    [updateValue]
  );

  const handleSavePress = useCallback(() => {
    save();
  }, [save]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
          ? translate('EditImportListExclusion')
          : translate('AddImportListExclusion')}
      </ModalHeader>

      <ModalBody>
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          <FormRow>
            <FormLabel>{translate('Title')}</FormLabel>

            <FormInputHelpText
              text={translate('SeriesTitleToExcludeHelpText')}
            />

            <FormInput
              type={inputTypes.TEXT}
              name="title"
              {...title}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('TvdbId')}</FormLabel>

            <FormInputHelpText text={translate('TvdbIdExcludeHelpText')} />

            <FormInput
              type={inputTypes.NUMBER}
              name="tvdbId"
              {...tvdbId}
              onChange={handleInputChange}
            />
          </FormRow>
        </Form>
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteImportListExclusionPress}
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

export default EditImportListExclusionModalContent;
