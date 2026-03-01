import React, { useCallback, useMemo, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import PathInput from 'Components/Form/PathInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds, sizes } from 'Helpers/Props';
import {
  addRecentFolder,
  useFavoriteFolders,
  useRecentFolders,
} from 'InteractiveImport/interactiveImportFoldersStore';
import translate from 'Utilities/String/translate';
import FavoriteFolderRow from './FavoriteFolderRow';
import RecentFolderRow from './RecentFolderRow';
import styles from './InteractiveImportSelectFolderModalContent.css';

const favoriteFoldersColumns: Column[] = [
  {
    name: 'folder',
    label: () => translate('Folder'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

const recentFoldersColumns: Column[] = [
  {
    name: 'folder',
    label: () => translate('Folder'),
    isVisible: true,
  },
  {
    name: 'lastUsed',
    label: () => translate('LastUsed'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

interface InteractiveImportSelectFolderModalContentProps {
  modalTitle: string;
  onFolderSelect(folder: string): void;
  onModalClose(): void;
}

function InteractiveImportSelectFolderModalContent(
  props: InteractiveImportSelectFolderModalContentProps
) {
  const { modalTitle, onFolderSelect, onModalClose } = props;
  const [folder, setFolder] = useState('');
  const executeCommand = useExecuteCommand();

  const favoriteFolders = useFavoriteFolders();
  const recentFolders = useRecentFolders();

  const favoriteFolderMap = useMemo(() => {
    return new Map(favoriteFolders.map((f) => [f.folder, f]));
  }, [favoriteFolders]);

  const onPathChange = useCallback(
    ({ value }: { value: string }) => {
      setFolder(value);
    },
    [setFolder]
  );

  const onRecentPathPress = useCallback(
    (value: string) => {
      setFolder(value);
    },
    [setFolder]
  );

  const onQuickImportPress = useCallback(() => {
    addRecentFolder(folder);

    executeCommand({
      name: CommandNames.DownloadedEpisodesScan,
      path: folder,
    });

    onModalClose();
  }, [folder, onModalClose, executeCommand]);

  const onInteractiveImportPress = useCallback(() => {
    addRecentFolder(folder);
    onFolderSelect(folder);
  }, [folder, onFolderSelect]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectFolderModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody>
        <PathInput
          name="folder"
          value={folder}
          includeFiles={false}
          onChange={onPathChange}
        />

        {favoriteFolders.length ? (
          <div className={styles.foldersContainer}>
            <div className={styles.foldersTitle}>
              {translate('FavoriteFolders')}
            </div>

            <Table columns={favoriteFoldersColumns}>
              <TableBody>
                {favoriteFolders.map((favoriteFolder) => {
                  return (
                    <FavoriteFolderRow
                      key={favoriteFolder.folder}
                      folder={favoriteFolder.folder}
                      onPress={onRecentPathPress}
                    />
                  );
                })}
              </TableBody>
            </Table>
          </div>
        ) : null}

        {recentFolders.length ? (
          <div className={styles.foldersContainer}>
            <div className={styles.foldersTitle}>
              {translate('RecentFolders')}
            </div>

            <Table columns={recentFoldersColumns}>
              <TableBody>
                {recentFolders
                  .slice(0)
                  .reverse()
                  .map((recentFolder) => {
                    return (
                      <RecentFolderRow
                        key={recentFolder.folder}
                        folder={recentFolder.folder}
                        lastUsed={recentFolder.lastUsed}
                        isFavorite={favoriteFolderMap.has(recentFolder.folder)}
                        onPress={onRecentPathPress}
                      />
                    );
                  })}
              </TableBody>
            </Table>
          </div>
        ) : null}

        <div className={styles.buttonsContainer}>
          <div className={styles.buttonContainer}>
            <Button
              className={styles.button}
              kind={kinds.PRIMARY}
              size={sizes.LARGE}
              isDisabled={!folder}
              onPress={onQuickImportPress}
            >
              <Icon className={styles.buttonIcon} name={icons.QUICK} />
              {translate('MoveAutomatically')}
            </Button>
          </div>

          <div className={styles.buttonContainer}>
            <Button
              className={styles.button}
              kind={kinds.PRIMARY}
              size={sizes.LARGE}
              isDisabled={!folder}
              onPress={onInteractiveImportPress}
            >
              <Icon className={styles.buttonIcon} name={icons.INTERACTIVE} />
              {translate('InteractiveImport')}
            </Button>
          </div>
        </div>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default InteractiveImportSelectFolderModalContent;
