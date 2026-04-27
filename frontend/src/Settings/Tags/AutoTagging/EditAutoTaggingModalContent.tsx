import React, { useCallback, useEffect, useState } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
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
import AddSpecificationModal from './Specifications/AddSpecificationModal';
import EditSpecificationModal from './Specifications/EditSpecificationModal';
import Specification from './Specifications/Specification';
import {
  AutoTagging,
  AutoTaggingSpecification,
  useManageAutoTagging,
} from './useAutoTaggings';
import styles from './EditAutoTaggingModalContent.css';

export interface EditAutoTaggingModalContentProps {
  id?: number;
  cloneId?: number;
  onModalClose: () => void;
  onDeleteAutoTaggingPress?: () => void;
}

export default function EditAutoTaggingModalContent({
  id,
  cloneId,
  onModalClose,
  onDeleteAutoTaggingPress,
}: EditAutoTaggingModalContentProps) {
  const {
    item,
    validationErrors,
    validationWarnings,
    updateValue,
    saveAutoTagging,
    isSaving,
    saveError,
    specifications,
    saveSpecification,
    deleteSpecification,
    cloneSpecification,
  } = useManageAutoTagging(id, cloneId);

  const [isAddSpecificationModalOpen, setIsAddSpecificationModalOpen] =
    useState(false);
  const [isEditSpecificationModalOpen, setIsEditSpecificationModalOpen] =
    useState(false);
  const [editingSpecification, setEditingSpecification] =
    useState<AutoTaggingSpecification | null>(null);

  const handleAddSpecificationPress = useCallback(() => {
    setIsAddSpecificationModalOpen(true);
  }, []);

  const handleAddSpecificationModalClose = useCallback(
    (selectedSpec?: AutoTaggingSpecification) => {
      setIsAddSpecificationModalOpen(false);

      if (selectedSpec) {
        setEditingSpecification({ ...selectedSpec, id: 0 });
        setIsEditSpecificationModalOpen(true);
      }
    },
    []
  );

  const handleNewSpecificationModalClose = useCallback(() => {
    setIsEditSpecificationModalOpen(false);
    setEditingSpecification(null);
  }, []);

  const handleSaveNewSpecification = useCallback(
    (spec: AutoTaggingSpecification) => {
      saveSpecification(spec);
    },
    [saveSpecification]
  );

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      updateValue(
        name as keyof AutoTagging,
        value as AutoTagging[keyof AutoTagging]
      );
    },
    [updateValue]
  );

  const handleSavePress = useCallback(() => {
    saveAutoTagging();
  }, [saveAutoTagging]);

  const wasSaving = usePrevious(isSaving);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  const { name, removeTagsAutomatically, tags } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditAutoTag') : translate('AddAutoTag')}
      </ModalHeader>

      <ModalBody>
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
              <FormLabel>{translate('RemoveTagsAutomatically')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="removeTagsAutomatically"
                helpText={translate('RemoveTagsAutomaticallyHelpText')}
                {...removeTagsAutomatically}
                onChange={handleInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Tags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TAG}
                name="tags"
                onChange={handleInputChange}
                {...tags}
              />
            </FormGroup>
          </Form>

          <FieldSet legend={translate('Conditions')}>
            <div className={styles.autoTaggings}>
              {specifications.map((specification) => {
                return (
                  <Specification
                    key={specification.id}
                    {...specification}
                    onSaveSpecification={saveSpecification}
                    onCloneSpecificationPress={cloneSpecification}
                    onConfirmDeleteSpecification={deleteSpecification}
                  />
                );
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
            onModalClose={handleAddSpecificationModalClose}
          />

          {editingSpecification ? (
            <EditSpecificationModal
              isOpen={isEditSpecificationModalOpen}
              specification={editingSpecification}
              onSave={handleSaveNewSpecification}
              onModalClose={handleNewSpecificationModalClose}
            />
          ) : null}
        </div>
      </ModalBody>
      <ModalFooter>
        <div className={styles.rightButtons}>
          {id ? (
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteAutoTaggingPress}
            >
              {translate('Delete')}
            </Button>
          ) : null}
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
