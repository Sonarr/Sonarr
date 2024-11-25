import React, { useCallback } from 'react';
import Label, { LabelProps } from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import MiddleTruncate from 'Components/MiddleTruncate';
import { icons } from 'Helpers/Props';
import { TagBase } from './TagInput';
import styles from './TagInputTag.css';

export interface DeletedTag<T extends TagBase> {
  index: number;
  id: T['id'];
}

export interface EditedTag<T extends TagBase> {
  index: number;
  id: T['id'];
  value: T['name'];
}

export interface TagInputTagProps<T extends TagBase> {
  index: number;
  tag: T;
  kind: LabelProps['kind'];
  canEdit: boolean;
  onDelete: (deletedTag: DeletedTag<T>) => void;
  onEdit: (editedTag: EditedTag<T>) => void;
}

function TagInputTag<T extends TagBase>({
  tag,
  kind,
  index,
  canEdit,
  onDelete,
  onEdit,
}: TagInputTagProps<T>) {
  const handleDelete = useCallback(() => {
    onDelete({
      index,
      id: tag.id,
    });
  }, [index, tag, onDelete]);

  const handleEdit = useCallback(() => {
    onEdit({
      index,
      id: tag.id,
      value: tag.name,
    });
  }, [index, tag, onEdit]);

  return (
    <div className={styles.tag} tabIndex={-1}>
      <Label className={styles.label} kind={kind}>
        <Link
          className={canEdit ? styles.linkWithEdit : styles.link}
          tabIndex={-1}
          onPress={handleDelete}
        >
          <MiddleTruncate text={String(tag.name)} />
        </Link>

        {canEdit ? (
          <div className={styles.editContainer}>
            <IconButton
              className={styles.editButton}
              name={icons.EDIT}
              size={9}
              onPress={handleEdit}
            />
          </div>
        ) : null}
      </Label>
    </div>
  );
}

export default TagInputTag;
