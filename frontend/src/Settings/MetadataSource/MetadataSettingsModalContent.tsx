import React, { useCallback, useState } from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { LanguageSelectInputOnChangeProps } from 'Components/Form/Select/LanguageSelectInput';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { getValidationFailures } from 'Helpers/Hooks/useApiMutation';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import {
  useMetadataSourceSettingsValues,
  useSaveMetadataSourceSettings,
} from './useMetadataSourceSettings';

export interface MetadataSettingsModalContentProps {
  onModalClose: () => void;
}

function MetadataSettingsModalContent({
  onModalClose,
}: MetadataSettingsModalContentProps) {
  const { preferredMetadataLanguage } = useMetadataSourceSettingsValues();
  const { saveSettings, isSaving, saveError } =
    useSaveMetadataSourceSettings(onModalClose);
  const [languageId, setLanguageId] = useState(preferredMetadataLanguage);

  const { errors, warnings } = getValidationFailures(saveError);

  const handleInputChange = useCallback(
    ({ value }: LanguageSelectInputOnChangeProps) => {
      setLanguageId(value as number);
    },
    []
  );

  const handleSavePress = useCallback(() => {
    saveSettings({ preferredMetadataLanguage: languageId });
  }, [languageId, saveSettings]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('MetadataSettings')}</ModalHeader>

      <ModalBody>
        {saveError ? (
          <Alert kind={kinds.DANGER}>
            {translate('MetadataSettingsSaveError')}
          </Alert>
        ) : null}

        <Form validationErrors={errors} validationWarnings={warnings}>
          <FormRow>
            <FormLabel>{translate('PreferredMetadataLanguage')}</FormLabel>

            <FormInputHelpText
              text={translate('PreferredMetadataLanguageHelpText')}
            />
            <FormInput
              type={inputTypes.LANGUAGE_SELECT}
              name="preferredMetadataLanguage"
              value={languageId}
              includeAny={false}
              onChange={handleInputChange}
            />
          </FormRow>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerButton
          kind={kinds.PRIMARY}
          isSpinning={isSaving}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default MetadataSettingsModalContent;
