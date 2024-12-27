import React, { useCallback, useState } from 'react';
import { Tag } from 'App/State/TagsAppState';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { icons, kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { AutoTaggingSpecification } from 'typings/AutoTagging';
import translate from 'Utilities/String/translate';
import EditAutoTaggingModal from './EditAutoTaggingModal';
import styles from './AutoTagging.css';

interface AutoTaggingProps {
  id: number;
  name: string;
  specifications: AutoTaggingSpecification[];
  tags: number[];
  tagList: Tag[];
  isDeleting: boolean;
  onConfirmDeleteAutoTagging: (id: number) => void;
  onCloneAutoTaggingPress: (id: number) => void;
}

export default function AutoTagging({
  id,
  name,
  tags,
  tagList,
  specifications,
  isDeleting,
  onConfirmDeleteAutoTagging,
  onCloneAutoTaggingPress,
}: AutoTaggingProps) {
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
    onConfirmDeleteAutoTagging(id);
  }, [id, onConfirmDeleteAutoTagging]);

  const onClonePress = useCallback(() => {
    onCloneAutoTaggingPress(id);
  }, [id, onCloneAutoTaggingPress]);

  return (
    <Card
      className={styles.autoTagging}
      overlayContent={true}
      onPress={onEditPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <div>
          <IconButton
            className={styles.cloneButton}
            title={translate('CloneAutoTag')}
            name={icons.CLONE}
            onPress={onClonePress}
          />
        </div>
      </div>

      <TagList tags={tags} tagList={tagList} />

      <div>
        {specifications.map((item, index) => {
          if (!item) {
            return null;
          }

          let kind: Kind = 'default';

          if (item.required) {
            kind = 'success';
          }
          if (item.negate) {
            kind = 'danger';
          }

          return (
            <Label key={index} kind={kind}>
              {item.name}
            </Label>
          );
        })}
      </div>

      <EditAutoTaggingModal
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={onEditModalClose}
        onDeleteAutoTaggingPress={onDeletePress}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteAutoTag')}
        message={translate('DeleteAutoTagHelpText', { name })}
        confirmLabel={translate('Delete')}
        isSpinning={isDeleting}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </Card>
  );
}
