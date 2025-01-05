import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import {
  fetchCustomFormatSpecifications,
  saveCustomFormat,
  setCustomFormatValue,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import ImportCustomFormatModal from './ImportCustomFormatModal';
import AddSpecificationModal from './Specifications/AddSpecificationModal';
import EditSpecificationModal from './Specifications/EditSpecificationModal';
import Specification from './Specifications/Specification';
import styles from './EditCustomFormatModalContent.css';

export interface EditCustomFormatModalContentProps {
  id?: number;
  clonedId?: number;
  onDeleteCustomFormatPress?: () => void;
  onModalClose: () => void;
}

function EditCustomFormatModalContent({
  id,
  clonedId,
  onDeleteCustomFormatPress,
  onModalClose,
}: EditCustomFormatModalContentProps) {
  const dispatch = useDispatch();

  const {
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    validationErrors,
    validationWarnings,
  } = useSelector(createProviderSettingsSelectorHook('customFormats', id));

  const { isPopulated: isSpecificationsPopulated, items: specifications } =
    useSelector((state: AppState) => state.settings.customFormatSpecifications);

  const [isAddSpecificationModalOpen, setIsAddSpecificationModalOpen] =
    useState(false);
  const [isEditSpecificationModalOpen, setIsEditSpecificationModalOpen] =
    useState(false);
  const [isImportCustomFormatModalOpen, setIsImportCustomFormatModalOpen] =
    useState(false);

  const { name, includeCustomFormatWhenRenaming } = item;
  const wasSaving = usePrevious(isSaving);

  const handleAddSpecificationPress = useCallback(() => {
    setIsAddSpecificationModalOpen(true);
  }, []);

  const handleAddSpecificationModalClose = useCallback(() => {
    setIsAddSpecificationModalOpen(false);
  }, []);

  const handleSpecificationSelect = useCallback(() => {
    setIsAddSpecificationModalOpen(false);
    setIsEditSpecificationModalOpen(true);
  }, []);

  const handleEditSpecificationModalClose = useCallback(() => {
    setIsEditSpecificationModalOpen(false);
  }, []);

  const handleImportPress = useCallback(() => {
    setIsImportCustomFormatModalOpen(true);
  }, []);

  const handleImportCustomFormatModalClose = useCallback(() => {
    setIsImportCustomFormatModalOpen(false);
  }, []);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setCustomFormatValue(change));
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(saveCustomFormat({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    dispatch(fetchCustomFormatSpecifications({ id: clonedId || id }));
  }, [id, clonedId, dispatch]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditCustomFormat') : translate('AddCustomFormat')}
      </ModalHeader>

      <ModalBody>
        <div>
          {isFetching ? <LoadingIndicator /> : null}

          {!isFetching && error ? (
            <Alert kind={kinds.DANGER}>
              {translate('AddCustomFormatError')}
            </Alert>
          ) : null}

          {!isFetching && !error && isSpecificationsPopulated ? (
            <div>
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
                    onChange={handleInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>
                    {translate('IncludeCustomFormatWhenRenaming')}
                  </FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="includeCustomFormatWhenRenaming"
                    helpText={translate(
                      'IncludeCustomFormatWhenRenamingHelpText'
                    )}
                    {...includeCustomFormatWhenRenaming}
                    onChange={handleInputChange}
                  />
                </FormGroup>
              </Form>

              <FieldSet legend={translate('Conditions')}>
                <Alert kind={kinds.INFO}>
                  <div>{translate('CustomFormatsSettingsTriggerInfo')}</div>
                </Alert>

                <div className={styles.customFormats}>
                  {specifications.map((tag) => {
                    return <Specification key={tag.id} {...tag} />;
                  })}

                  <Card
                    className={styles.addSpecification}
                    onPress={handleAddSpecificationPress}
                  >
                    <div className={styles.center}>
                      <Icon name={icons.ADD} size={45} />
                    </div>
                  </Card>
                </div>
              </FieldSet>

              <AddSpecificationModal
                isOpen={isAddSpecificationModalOpen}
                onSpecificationSelect={handleSpecificationSelect}
                onModalClose={handleAddSpecificationModalClose}
              />

              <EditSpecificationModal
                isOpen={isEditSpecificationModalOpen}
                onModalClose={handleEditSpecificationModalClose}
              />

              <ImportCustomFormatModal
                isOpen={isImportCustomFormatModalOpen}
                onModalClose={handleImportCustomFormatModalClose}
              />
            </div>
          ) : null}
        </div>
      </ModalBody>

      <ModalFooter>
        <div className={styles.rightButtons}>
          {id ? (
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteCustomFormatPress}
            >
              {translate('Delete')}
            </Button>
          ) : null}

          <Button className={styles.deleteButton} onPress={handleImportPress}>
            {translate('Import')}
          </Button>
        </div>

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

export default EditCustomFormatModalContent;
