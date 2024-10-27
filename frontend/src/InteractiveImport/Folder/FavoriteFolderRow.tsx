import React, { SyntheticEvent, useCallback } from 'react';
import { useDispatch } from 'react-redux';
import IconButton from 'Components/Link/IconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowButton from 'Components/Table/TableRowButton';
import { icons } from 'Helpers/Props';
import { removeFavoriteFolder } from 'Store/Actions/interactiveImportActions';
import translate from 'Utilities/String/translate';
import styles from './FavoriteFolderRow.css';

interface FavoriteFolderRowProps {
  folder: string;
  onPress: (folder: string) => unknown;
}

function FavoriteFolderRow({ folder, onPress }: FavoriteFolderRowProps) {
  const dispatch = useDispatch();

  const handlePress = useCallback(() => {
    onPress(folder);
  }, [folder, onPress]);

  const handleRemoveFavoritePress = useCallback(
    (e: SyntheticEvent) => {
      e.stopPropagation();

      dispatch(removeFavoriteFolder({ folder }));
    },
    [folder, dispatch]
  );

  return (
    <TableRowButton onPress={handlePress}>
      <TableRowCell>{folder}</TableRowCell>

      <TableRowCell className={styles.actions}>
        <IconButton
          title={translate('FavoriteFolderRemove')}
          kind="danger"
          name={icons.HEART}
          onPress={handleRemoveFavoritePress}
        />
      </TableRowCell>
    </TableRowButton>
  );
}

export default FavoriteFolderRow;
