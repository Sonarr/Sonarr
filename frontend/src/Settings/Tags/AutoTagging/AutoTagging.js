import PropTypes from 'prop-types';
import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditAutoTaggingModal from './EditAutoTaggingModal';
import styles from './AutoTagging.css';

export default function AutoTagging(props) {
  const {
    id,
    name,
    tags,
    tagList,
    specifications,
    isDeleting,
    onConfirmDeleteAutoTagging,
    onCloneAutoTaggingPress
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
        <div className={styles.name}>
          {name}
        </div>

        <div>
          <IconButton
            className={styles.cloneButton}
            title={translate('CloneAutoTag')}
            name={icons.CLONE}
            onPress={onClonePress}
          />
        </div>
      </div>

      <TagList
        tags={tags}
        tagList={tagList}
      />

      <div>
        {
          specifications.map((item, index) => {
            if (!item) {
              return null;
            }

            let kind = kinds.DEFAULT;
            if (item.required) {
              kind = kinds.SUCCESS;
            }
            if (item.negate) {
              kind = kinds.DANGER;
            }

            return (
              <Label
                key={index}
                kind={kind}
              >
                {item.name}
              </Label>
            );
          })
        }
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

AutoTagging.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  specifications: PropTypes.arrayOf(PropTypes.object).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteAutoTagging: PropTypes.func.isRequired,
  onCloneAutoTaggingPress: PropTypes.func.isRequired
};
