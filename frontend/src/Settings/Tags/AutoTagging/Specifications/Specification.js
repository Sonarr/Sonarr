import PropTypes from 'prop-types';
import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditSpecificationModal from './EditSpecificationModal';
import styles from './Specification.css';

export default function Specification(props) {
  const {
    id,
    implementationName,
    name,
    required,
    negate,
    onConfirmDeleteSpecification,
    onCloneSpecificationPress
  } = props;

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onDeletePress = useCallback(() => {
    setIsEditModalOpen(false);
    setIsDeleteModalOpen(true);
  }, [setIsEditModalOpen, setIsDeleteModalOpen]);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, [setIsDeleteModalOpen]);

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
        <div className={styles.name}>
          {name}
        </div>

        <IconButton
          className={styles.cloneButton}
          title={translate('Clone')}
          name={icons.CLONE}
          onPress={onClonePress}
        />
      </div>

      <div className={styles.labels}>
        <Label kind={kinds.DEFAULT}>
          {implementationName}
        </Label>

        {
          negate ?
            <Label kind={kinds.DANGER}>
              {translate('Negated')}
            </Label> :
            null
        }

        {
          required ?
            <Label kind={kinds.SUCCESS}>
              {translate('Required')}
            </Label> :
            null
        }
      </div>

      <EditSpecificationModal
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={onEditModalClose}
        onDeleteSpecificationPress={onDeletePress}
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

Specification.propTypes = {
  id: PropTypes.number.isRequired,
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  negate: PropTypes.bool.isRequired,
  required: PropTypes.bool.isRequired,
  fields: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteSpecification: PropTypes.func.isRequired,
  onCloneSpecificationPress: PropTypes.func.isRequired
};
