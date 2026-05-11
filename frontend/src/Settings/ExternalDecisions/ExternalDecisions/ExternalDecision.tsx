import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { kinds } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import {
  ExternalDecisionModel,
  useDeleteExternalDecision,
} from '../useExternalDecisions';
import EditExternalDecisionModal from './EditExternalDecisionModal';
import styles from './ExternalDecision.css';

interface ExternalDecisionProps extends ExternalDecisionModel {
  showPriority: boolean;
}

function ExternalDecision({
  id,
  name,
  enable,
  decisionType,
  priority,
  tags,
  showPriority,
}: ExternalDecisionProps) {
  const tagList = useTagList();
  const { deleteExternalDecision } = useDeleteExternalDecision(id);

  const [isEditExternalDecisionModalOpen, setIsEditExternalDecisionModalOpen] =
    useState(false);
  const [
    isDeleteExternalDecisionModalOpen,
    setIsDeleteExternalDecisionModalOpen,
  ] = useState(false);

  const handleEditExternalDecisionPress = useCallback(() => {
    setIsEditExternalDecisionModalOpen(true);
  }, []);

  const handleEditExternalDecisionModalClose = useCallback(() => {
    setIsEditExternalDecisionModalOpen(false);
  }, []);

  const handleDeleteExternalDecisionPress = useCallback(() => {
    setIsEditExternalDecisionModalOpen(false);
    setIsDeleteExternalDecisionModalOpen(true);
  }, []);

  const handleDeleteExternalDecisionModalClose = useCallback(() => {
    setIsDeleteExternalDecisionModalOpen(false);
  }, []);

  const handleConfirmDeleteExternalDecision = useCallback(() => {
    deleteExternalDecision();
  }, [deleteExternalDecision]);

  return (
    <Card
      className={styles.externalDecision}
      overlayContent={true}
      onPress={handleEditExternalDecisionPress}
    >
      <div className={styles.name}>{name}</div>

      <Label kind={enable ? kinds.SUCCESS : kinds.DISABLED}>
        {translate(`ExternalDecisionType${titleCase(decisionType)}`)}
      </Label>

      {showPriority ? (
        <Label kind={kinds.DEFAULT}>
          {translate('PrioritySettings', { priority })}
        </Label>
      ) : null}

      {enable ? null : (
        <Label kind={kinds.DISABLED} outline={true}>
          {translate('Disabled')}
        </Label>
      )}

      <TagList tags={tags} tagList={tagList} />

      <EditExternalDecisionModal
        id={id}
        isOpen={isEditExternalDecisionModalOpen}
        onModalClose={handleEditExternalDecisionModalClose}
        onDeleteExternalDecisionPress={handleDeleteExternalDecisionPress}
      />

      <ConfirmModal
        isOpen={isDeleteExternalDecisionModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteExternalDecision')}
        message={translate('DeleteExternalDecisionMessageText', { name })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteExternalDecision}
        onCancel={handleDeleteExternalDecisionModalClose}
      />
    </Card>
  );
}

export default ExternalDecision;
