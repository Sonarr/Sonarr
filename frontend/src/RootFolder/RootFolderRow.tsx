import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Button from 'Components/Link/Button';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds, sizes } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import { RootFolder, useDeleteRootFolder } from './useRootFolders';
import styles from './RootFolderRow.css';

interface RootFolderRowProps extends RootFolder {
  isSelectable: boolean;
}

function renderBadge(isUnavailable: boolean, isEmpty: boolean) {
  if (isUnavailable) {
    return (
      <Label kind={kinds.DANGER} dot={false}>
        {translate('Unavailable')}
      </Label>
    );
  }

  if (isEmpty) {
    return (
      <Label
        kind={kinds.WARNING}
        dot={false}
        title={translate('EmptyRootFolderTooltip')}
      >
        {translate('Empty')}
      </Label>
    );
  }

  return null;
}

function RootFolderRow(props: RootFolderRowProps) {
  const {
    id,
    path,
    accessible,
    isEmpty,
    freeSpace,
    isSelectable,
    unmappedFolders = [],
  } = props;

  const isUnavailable = !accessible;
  const importableCount = unmappedFolders.length;
  const hasFreeSpace = freeSpace != null && !isNaN(freeSpace);

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const { deleteRootFolder } = useDeleteRootFolder(id);

  const handleDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, []);

  const handleDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  const handleConfirmDelete = useCallback(() => {
    deleteRootFolder();
    setIsDeleteModalOpen(false);
  }, [deleteRootFolder]);

  return (
    <div
      className={classNames(
        styles.row,
        isSelectable && styles.selectable,
        isUnavailable && styles.unavailable
      )}
    >
      {isSelectable && !isUnavailable ? (
        <Link
          className={styles.underlay}
          to={`/add/import/${id}`}
          aria-label={translate('ImportFromRootFolder', { path })}
        />
      ) : null}

      <div
        className={classNames(styles.content, isSelectable && styles.overlay)}
      >
        <Icon
          className={styles.folderIcon}
          name={icons.ROOT_FOLDER}
          size={20}
        />

        <div className={styles.info}>
          <span className={styles.path}>{path}</span>

          <div className={styles.meta}>
            {isUnavailable ? (
              <span>{translate('RootFolderNotAccessible')}</span>
            ) : (
              <>
                <span className={styles.importable}>
                  {importableCount > 0
                    ? translate('RootFolderFoldersToImport', {
                        count: importableCount,
                      })
                    : translate('RootFolderNothingToImport')}
                </span>

                {hasFreeSpace ? (
                  <>
                    <span className={styles.metaSep}>·</span>

                    <span className={styles.free}>
                      {translate('RootFolderSelectFreeSpace', {
                        freeSpace: formatBytes(freeSpace),
                      })}
                    </span>
                  </>
                ) : null}
              </>
            )}
          </div>
        </div>

        {renderBadge(isUnavailable, isEmpty)}

        {isSelectable || isUnavailable ? null : (
          <Button
            size={sizes.SMALL}
            to={`/add/import/${id}`}
            aria-label={translate('ImportFromRootFolder', { path })}
          >
            {translate('Import')}
          </Button>
        )}

        <IconButton
          title={translate('RemoveRootFolder')}
          aria-label={translate('RemoveRootFolder')}
          name={icons.REMOVE}
          onPress={handleDeletePress}
        />
      </div>

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('RemoveRootFolder')}
        message={translate('RemoveRootFolderWithSeriesMessageText', { path })}
        confirmLabel={translate('Remove')}
        onConfirm={handleConfirmDelete}
        onCancel={handleDeleteModalClose}
      />
    </div>
  );
}

export default RootFolderRow;
