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
import useDownloadClientHostOptions from 'DownloadClient/useDownloadClientHostOptions';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useManageRemotePathMappings } from './useRemotePathMappings';
import styles from './EditRemotePathMappingModalContent.css';

export interface EditRemotePathMappingModalContentProps {
  id?: number;
  onDeleteRemotePathMappingPress?: () => void;
  onModalClose: () => void;
}

function EditRemotePathMappingModalContent({
  id,
  onDeleteRemotePathMappingPress,
  onModalClose,
}: EditRemotePathMappingModalContentProps) {
  const downloadClientHosts = useDownloadClientHostOptions();

  const {
    item,
    isSaving,
    saveError,
    validationErrors,
    validationWarnings,
    updateValue,
    saveProvider,
  } = useManageRemotePathMappings(id ?? 0);

  const { host, remotePath, localPath } = item;
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

  // useEffect(() => {
  //   if (!id) {
  //     Object.keys(newRemotePathMapping).forEach((name) => {
  //       if (name === 'id') {
  //         return;
  //       }

  //       dispatch(
  //         // @ts-expect-error - actions are not typed
  //         setRemotePathMappingValue({
  //           name,
  //           value: newRemotePathMapping[name],
  //         })
  //       );
  //     });
  //   }
  // }, [id, dispatch]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
          ? translate('EditRemotePathMapping')
          : translate('AddRemotePathMapping')}
      </ModalHeader>

      <ModalBody className={styles.body}>
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          <FormGroup>
            <FormLabel>{translate('Host')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="host"
              helpText={translate('RemotePathMappingHostHelpText')}
              {...host}
              values={downloadClientHosts}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('RemotePath')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="remotePath"
              helpText={translate('RemotePathMappingRemotePathHelpText')}
              {...remotePath}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('LocalPath')}</FormLabel>

            <FormInputGroup
              type={inputTypes.PATH}
              name="localPath"
              helpText={translate('RemotePathMappingLocalPathHelpText')}
              includeFiles={false}
              {...localPath}
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
            onPress={onDeleteRemotePathMappingPress}
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

export default EditRemotePathMappingModalContent;
