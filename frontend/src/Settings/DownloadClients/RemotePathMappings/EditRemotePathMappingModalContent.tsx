import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import useDownloadClientHostOptions from 'DownloadClient/useDownloadClientHostOptions';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import {
  saveRemotePathMapping,
  setRemotePathMappingValue,
} from 'Store/Actions/settingsActions';
import selectSettings from 'Store/Selectors/selectSettings';
import { InputChanged } from 'typings/inputs';
import RemotePathMapping from 'typings/Settings/RemotePathMapping';
import translate from 'Utilities/String/translate';
import styles from './EditRemotePathMappingModalContent.css';

const newRemotePathMapping: RemotePathMapping & { [key: string]: unknown } = {
  id: 0,
  host: '',
  remotePath: '',
  localPath: '',
};

function createRemotePathMappingSelector(id?: number) {
  return createSelector(
    (state: AppState) => state.settings.remotePathMappings,
    (remotePathMappings) => {
      const { isFetching, error, isSaving, saveError, pendingChanges, items } =
        remotePathMappings;

      const mapping = id
        ? items.find((i) => i.id === id)!
        : newRemotePathMapping;

      const settings = selectSettings(mapping, pendingChanges, saveError);

      return {
        id,
        isFetching,
        error,
        isSaving,
        saveError,
        item: settings.settings,
        ...settings,
      };
    }
  );
}

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
  const dispatch = useDispatch();
  const downloadClientHosts = useDownloadClientHostOptions();

  const {
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    validationErrors,
    validationWarnings,
  } = useSelector(createRemotePathMappingSelector(id));

  const { host, remotePath, localPath } = item;
  const wasSaving = usePrevious(isSaving);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setRemotePathMappingValue(change));
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(saveRemotePathMapping({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    if (!id) {
      Object.keys(newRemotePathMapping).forEach((name) => {
        if (name === 'id') {
          return;
        }

        dispatch(
          // @ts-expect-error - actions are not typed
          setRemotePathMappingValue({
            name,
            value: newRemotePathMapping[name],
          })
        );
      });
    }
  }, [id, dispatch]);

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
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>
            {translate('AddRemotePathMappingError')}
          </Alert>
        ) : null}

        {!isFetching && !error ? (
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
        ) : null}
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
