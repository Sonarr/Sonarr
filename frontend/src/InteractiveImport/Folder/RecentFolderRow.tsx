import React, { SyntheticEvent, useCallback } from 'react';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowButton from 'Components/Table/TableRowButton';
import { icons } from 'Helpers/Props';
import {
  addFavoriteFolder,
  removeFavoriteFolder,
  removeRecentFolder,
} from 'InteractiveImport/interactiveImportFoldersStore';
import translate from 'Utilities/String/translate';
import styles from './RecentFolderRow.module.css';

interface RecentFolderRowProps {
  folder: string;
  lastUsed: string;
  isFavorite: boolean;
  onPress: (folder: string) => unknown;
}

function RecentFolderRow({
  folder,
  lastUsed,
  isFavorite,
  onPress,
}: RecentFolderRowProps) {
  const handlePress = useCallback(() => {
    onPress(folder);
  }, [folder, onPress]);

  const handleFavoritePress = useCallback(
    (e: SyntheticEvent) => {
      e.stopPropagation();

      if (isFavorite) {
        removeFavoriteFolder(folder);
      } else {
        addFavoriteFolder(folder);
      }
    },
    [folder, isFavorite]
  );

  const handleRemovePress = useCallback(
    (e: SyntheticEvent) => {
      e.stopPropagation();

      removeRecentFolder(folder);
    },
    [folder]
  );

  const favoriteLabel = isFavorite
    ? translate('RemoveFavoriteFolderWithPath', { path: folder })
    : translate('AddFavoriteFolderWithPath', { path: folder });
  const removeRecentLabel = translate('RemoveRecentFolderWithPath', {
    path: folder,
  });

  return (
    <TableRowButton onPress={handlePress}>
      <TableRowCell>{folder}</TableRowCell>

      <RelativeDateCell date={lastUsed} />

      <TableRowCell className={styles.actions}>
        <IconButton
          title={favoriteLabel}
          aria-label={favoriteLabel}
          kind={isFavorite ? 'danger' : 'default'}
          name={icons.HEART}
          filled={isFavorite}
          onPress={handleFavoritePress}
        />

        <IconButton
          title={removeRecentLabel}
          aria-label={removeRecentLabel}
          name={icons.REMOVE}
          onPress={handleRemovePress}
        />
      </TableRowCell>
    </TableRowButton>
  );
}

export default RecentFolderRow;
