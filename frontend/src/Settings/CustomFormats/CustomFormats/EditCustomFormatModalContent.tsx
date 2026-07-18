import classNames from 'classnames';
import React, { useCallback, useEffect, useState } from 'react';
import Card from 'Components/Card';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import ImportCustomFormatModal from './ImportCustomFormatModal';
import AddSpecificationModal from './Specifications/AddSpecificationModal';
import EditSpecificationModal from './Specifications/EditSpecificationModal';
import Specification from './Specifications/Specification';
import {
  CustomFormat,
  CustomFormatSpecification,
  useManageCustomFormat,
} from './useCustomFormats';
import styles from './EditCustomFormatModalContent.css';

export interface EditCustomFormatModalContentProps {
  id?: number;
  cloneId?: number;
  onDeleteCustomFormatPress?: () => void;
  onModalClose: () => void;
}

function EditCustomFormatModalContent({
  id,
  cloneId,
  onDeleteCustomFormatPress,
  onModalClose,
}: EditCustomFormatModalContentProps) {
  const {
    item,
    validationErrors,
    validationWarnings,
    updateValue,
    saveCustomFormat,
    isSaving,
    saveError,
    specifications,
    saveSpecification,
    deleteSpecification,
    cloneSpecification,
  } = useManageCustomFormat(id, cloneId);

  const [isAddSpecificationModalOpen, setIsAddSpecificationModalOpen] =
    useState(false);
  const [isEditSpecificationModalOpen, setIsEditSpecificationModalOpen] =
    useState(false);
  const [editingSpecification, setEditingSpecification] =
    useState<CustomFormatSpecification | null>(null);
  const [isImportCustomFormatModalOpen, setIsImportCustomFormatModalOpen] =
    useState(false);

  const { name, includeCustomFormatWhenRenaming } = item;
  const wasSaving = usePrevious(isSaving);

  const handleAddSpecificationPress = useCallback(() => {
    setIsAddSpecificationModalOpen(true);
  }, []);

  const handleAddSpecificationModalClose = useCallback(
    (selectedSpec?: CustomFormatSpecification) => {
      setIsAddSpecificationModalOpen(false);

      if (selectedSpec) {
        setEditingSpecification({ ...selectedSpec, id: 0 });
        setIsEditSpecificationModalOpen(true);
      }
    },
    []
  );

  const handleEditSpecificationModalClose = useCallback(() => {
    setIsEditSpecificationModalOpen(false);
    setEditingSpecification(null);
  }, []);

  const handleSaveNewSpecification = useCallback(
    (spec: CustomFormatSpecification) => {
      saveSpecification(spec);
    },
    [saveSpecification]
  );

  const handleImportPress = useCallback(() => {
    setIsImportCustomFormatModalOpen(true);
  }, []);

  const handleImportCustomFormatModalClose = useCallback(() => {
    setIsImportCustomFormatModalOpen(false);
  }, []);

  const handleImport = useCallback(
    (imported: CustomFormat) => {
      updateValue('name', imported.name);
      updateValue(
        'includeCustomFormatWhenRenaming',
        imported.includeCustomFormatWhenRenaming
      );
      updateValue('specifications', []);

      imported.specifications.forEach((spec, index) => {
        saveSpecification({ ...spec, id: index + 1 });
      });
    },
    [updateValue, saveSpecification]
  );

  const handleInputChange = useCallback(
    ({ name: key, value }: InputChanged) => {
      updateValue(
        key as keyof CustomFormat,
        value as CustomFormat[keyof CustomFormat]
      );
    },
    [updateValue]
  );

  const handleSavePress = useCallback(() => {
    saveCustomFormat();
  }, [saveCustomFormat]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
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
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>
                {translate('IncludeCustomFormatWhenRenaming')}
              </FormLabel>
              <FormInputHelpText
                text={translate('IncludeCustomFormatWhenRenamingHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="includeCustomFormatWhenRenaming"
                {...includeCustomFormatWhenRenaming}
                onChange={handleInputChange}
              />
            </FormRow>
          </Form>

          <div className={styles.conditions}>
            <h3 className={styles.conditionsTitle}>
              {translate('Conditions')}
            </h3>

            <p className={styles.intro}>
              {translate('CustomFormatsMatchInfo')}
            </p>

            <div className={styles.customFormats}>
              {specifications.map((specification) => (
                <Specification
                  key={specification.id}
                  {...specification}
                  onSaveSpecification={saveSpecification}
                  onCloneSpecificationPress={cloneSpecification}
                  onConfirmDeleteSpecification={deleteSpecification}
                />
              ))}

              <Card
                className={classNames(
                  styles.addSpecification,
                  !specifications.length && styles.fullWidth
                )}
                onPress={handleAddSpecificationPress}
              >
                <div className={styles.center}>
                  <Icon name={icons.ADD} size={20} />
                </div>
                <div className={styles.addLabel}>
                  {translate('AddCondition')}
                </div>
              </Card>
            </div>
          </div>

          <AddSpecificationModal
            isOpen={isAddSpecificationModalOpen}
            onModalClose={handleAddSpecificationModalClose}
          />

          {editingSpecification ? (
            <EditSpecificationModal
              isOpen={isEditSpecificationModalOpen}
              specification={editingSpecification}
              onSave={handleSaveNewSpecification}
              onModalClose={handleEditSpecificationModalClose}
            />
          ) : null}

          <ImportCustomFormatModal
            isOpen={isImportCustomFormatModalOpen}
            onImport={handleImport}
            onModalClose={handleImportCustomFormatModalClose}
          />
        </div>
      </ModalBody>
      <ModalFooter>
        <div className={styles.rightButtons}>
          {id ? (
            <Button kind={kinds.DANGER} onPress={onDeleteCustomFormatPress}>
              {translate('Delete')}
            </Button>
          ) : null}

          <Button onPress={handleImportPress}>{translate('Import')}</Button>
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
