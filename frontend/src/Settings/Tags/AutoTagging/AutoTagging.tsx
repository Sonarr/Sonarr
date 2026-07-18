import React, { useCallback, useState } from 'react';
import Label from 'Components/Label';
import MiddleTruncate from 'Components/MiddleTruncate';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import { icons, kinds, sizes } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { Tag } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import EditAutoTaggingModal from './EditAutoTaggingModal';
import {
  AutoTaggingSpecification,
  useDeleteAutoTagging,
} from './useAutoTaggings';

interface AutoTaggingProps {
  id: number;
  name: string;
  specifications: AutoTaggingSpecification[];
  tags: number[];
  tagList: ReadonlyArray<Tag>;
  onCloneAutoTaggingPress: (id: number) => void;
}

export default function AutoTagging({
  id,
  name,
  tags,
  tagList,
  specifications,
  onCloneAutoTaggingPress,
}: AutoTaggingProps) {
  const { deleteAutoTagging, isDeleting } = useDeleteAutoTagging(id);
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
    deleteAutoTagging();
  }, [deleteAutoTagging]);

  const onClonePress = useCallback(() => {
    onCloneAutoTaggingPress(id);
  }, [id, onCloneAutoTaggingPress]);

  return (
    <SettingsCard
      name={name}
      aria-label={translate('EditAutoTagName', { name })}
      actions={
        <SettingsCardAction
          title={translate('CloneAutoTag')}
          aria-label={translate('CloneAutoTag')}
          name={icons.CLONE}
          onPress={onClonePress}
        />
      }
      onPress={onEditPress}
    >
      <SettingsCardStatus
        segments={[
          translate('ConditionsCount', { count: specifications.length }),
        ]}
      />

      {tags.length ? <TagList tags={tags} tagList={tagList} /> : null}

      {specifications.length ? (
        <div className={settingsCardStyles.labels}>
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
              <Label
                key={index}
                className={settingsCardStyles.truncatedLabel}
                kind={kind}
                size={sizes.MEDIUM}
                dot={item.required || item.negate}
              >
                <MiddleTruncate text={item.name} />
              </Label>
            );
          })}
        </div>
      ) : null}

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
    </SettingsCard>
  );
}
