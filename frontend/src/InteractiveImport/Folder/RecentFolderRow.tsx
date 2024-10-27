import React, { SyntheticEvent, useCallback } from 'react';
import { useDispatch } from 'react-redux';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowButton from 'Components/Table/TableRowButton';
import { icons } from 'Helpers/Props';
import {
  addFavoriteFolder,
  removeFavoriteFolder,
  removeRecentFolder,
} from 'Store/Actions/interactiveImportActions';
import translate from 'Utilities/String/translate';
import styles from './RecentFolderRow.css';

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
  const dispatch = useDispatch();

  const handlePress = useCallback(() => {
    onPress(folder);
  }, [folder, onPress]);

  const handleFavoritePress = useCallback(
    (e: SyntheticEvent) => {
      e.stopPropagation();

      if (isFavorite) {
        dispatch(removeFavoriteFolder({ folder }));
      } else {
        dispatch(addFavoriteFolder({ folder }));
      }
    },
    [folder, isFavorite, dispatch]
  );

  const handleRemovePress = useCallback(
    (e: SyntheticEvent) => {
      e.stopPropagation();

      dispatch(removeRecentFolder({ folder }));
    },
    [folder, dispatch]
  );

  return (
    <TableRowButton onPress={handlePress}>
      <TableRowCell>{folder}</TableRowCell>

      <RelativeDateCell date={lastUsed} />

      <TableRowCell className={styles.actions}>
        <IconButton
          title={
            isFavorite
              ? translate('FavoriteFolderRemove')
              : translate('FavoriteFolderAdd')
          }
          kind={isFavorite ? 'danger' : 'default'}
          name={isFavorite ? icons.HEART : icons.HEART_OUTLINE}
          onPress={handleFavoritePress}
        />

        <IconButton
          title={translate('Remove')}
          name={icons.REMOVE}
          onPress={handleRemovePress}
        />
      </TableRowCell>
    </TableRowButton>
  );
}

export default RecentFolderRow;
