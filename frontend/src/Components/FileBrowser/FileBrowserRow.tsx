import React, { useCallback } from 'react';
import { PathType } from 'App/State/PathsAppState';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowButton from 'Components/Table/TableRowButton';
import { icons } from 'Helpers/Props';
import styles from './FileBrowserRow.css';

function getIconName(type: PathType) {
  switch (type) {
    case 'computer':
      return icons.COMPUTER;
    case 'drive':
      return icons.DRIVE;
    case 'file':
      return icons.FILE;
    case 'parent':
      return icons.PARENT;
    default:
      return icons.FOLDER;
  }
}

interface FileBrowserRowProps {
  type: PathType;
  name: string;
  path: string;
  onPress: (path: string) => void;
}

function FileBrowserRow(props: FileBrowserRowProps) {
  const { type, name, path, onPress } = props;

  const handlePress = useCallback(() => {
    onPress(path);
  }, [path, onPress]);

  return (
    <TableRowButton onPress={handlePress}>
      <TableRowCell className={styles.type}>
        <Icon name={getIconName(type)} />
      </TableRowCell>

      <TableRowCell>{name}</TableRowCell>
    </TableRowButton>
  );
}

export default FileBrowserRow;
