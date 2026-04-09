import React, { useCallback, useMemo, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import Field from 'typings/Field';
import translate from 'Utilities/String/translate';
import { AutoTaggingSpecification } from '../useAutoTaggings';
import EditSpecificationModal from './EditSpecificationModal';
import styles from './Specification.css';

interface SpecificationProps {
  id: number;
  implementation: string;
  implementationName: string;
  name: string;
  negate: boolean;
  required: boolean;
  fields: Field[];
  onSaveSpecification: (spec: AutoTaggingSpecification) => void;
  onConfirmDeleteSpecification: (specId: number) => void;
  onCloneSpecificationPress: (specId: number) => void;
}

export default function Specification({
  id,
  implementation,
  implementationName,
  name,
  negate,
  required,
  fields,
  onSaveSpecification,
  onConfirmDeleteSpecification,
  onCloneSpecificationPress,
}: SpecificationProps) {
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  const spec = useMemo<AutoTaggingSpecification>(
    () => ({
      id,
      implementation,
      implementationName,
      name,
      negate,
      required,
      fields,
    }),
    [id, implementation, implementationName, name, negate, required, fields]
  );

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, []);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, []);

  const onDeletePress = useCallback(() => {
    setIsEditModalOpen(false);
    setIsDeleteModalOpen(true);
  }, []);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  const onConfirmDelete = useCallback(() => {
    onConfirmDeleteSpecification(id);
  }, [id, onConfirmDeleteSpecification]);

  const onClonePress = useCallback(() => {
    onCloneSpecificationPress(id);
  }, [id, onCloneSpecificationPress]);

  return (
    <Card
      className={styles.autoTagging}
      overlayContent={true}
      onPress={onEditPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <IconButton
          className={styles.cloneButton}
          title={translate('Clone')}
          aria-label={translate('Clone')}
          name={icons.CLONE}
          onPress={onClonePress}
        />
      </div>

      <div className={styles.labels}>
        <Label kind={kinds.DEFAULT}>{implementationName}</Label>

        {negate ? (
          <Label kind={kinds.DANGER}>{translate('Negated')}</Label>
        ) : null}

        {required ? (
          <Label kind={kinds.SUCCESS}>{translate('Required')}</Label>
        ) : null}
      </div>

      <EditSpecificationModal
        isOpen={isEditModalOpen}
        specification={spec}
        onSave={onSaveSpecification}
        onDeleteSpecificationPress={onDeletePress}
        onModalClose={onEditModalClose}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteSpecification')}
        message={translate('DeleteSpecificationHelpText', { name })}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </Card>
  );
}
