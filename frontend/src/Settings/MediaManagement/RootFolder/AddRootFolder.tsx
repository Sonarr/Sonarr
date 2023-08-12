import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import { icons, kinds, sizes } from 'Helpers/Props';
import { addRootFolder } from 'Store/Actions/rootFolderActions';
import translate from 'Utilities/String/translate';
import styles from './AddRootFolder.css';

function AddRootFolder() {
  const dispatch = useDispatch();

  const [isAddNewRootFolderModalOpen, setIsAddNewRootFolderModalOpen] =
    useState(false);

  const onAddNewRootFolderPress = useCallback(() => {
    setIsAddNewRootFolderModalOpen(true);
  }, [setIsAddNewRootFolderModalOpen]);

  const onNewRootFolderSelect = useCallback(
    ({ value }: { value: string }) => {
      dispatch(addRootFolder({ path: value }));
    },
    [dispatch]
  );

  const onAddRootFolderModalClose = useCallback(() => {
    setIsAddNewRootFolderModalOpen(false);
  }, [setIsAddNewRootFolderModalOpen]);

  return (
    <div className={styles.addRootFolderButtonContainer}>
      <Button
        kind={kinds.PRIMARY}
        size={sizes.LARGE}
        onPress={onAddNewRootFolderPress}
      >
        <Icon className={styles.importButtonIcon} name={icons.DRIVE} />
        {translate('AddRootFolder')}
      </Button>

      <FileBrowserModal
        isOpen={isAddNewRootFolderModalOpen}
        name="rootFolderPath"
        value=""
        onChange={onNewRootFolderSelect}
        onModalClose={onAddRootFolderModalClose}
      />
    </div>
  );
}

export default AddRootFolder;
