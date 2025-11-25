import React, { useCallback, useState } from 'react';
import Alert from 'Components/Alert';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import { icons, kinds, sizes } from 'Helpers/Props';
import { useAddRootFolder } from 'RootFolder/useRootFolders';
import translate from 'Utilities/String/translate';
import styles from './AddRootFolder.css';

function AddRootFolder() {
  const { addRootFolder, isAdding, addError } = useAddRootFolder();

  const [isAddNewRootFolderModalOpen, setIsAddNewRootFolderModalOpen] =
    useState(false);

  const onAddNewRootFolderPress = useCallback(() => {
    setIsAddNewRootFolderModalOpen(true);
  }, [setIsAddNewRootFolderModalOpen]);

  const onNewRootFolderSelect = useCallback(
    ({ value }: { value: string }) => {
      addRootFolder({ path: value });
    },
    [addRootFolder]
  );

  const onAddRootFolderModalClose = useCallback(() => {
    setIsAddNewRootFolderModalOpen(false);
  }, [setIsAddNewRootFolderModalOpen]);

  return (
    <>
      {!isAdding && addError ? (
        <Alert kind={kinds.DANGER}>
          {translate('AddRootFolderError')}

          <ul>
            {Array.isArray(addError.statusBody) ? (
              addError.statusBody.map((e, index) => {
                return <li key={index}>{e.errorMessage}</li>;
              })
            ) : (
              <li>{JSON.stringify(addError.statusBody)}</li>
            )}
          </ul>
        </Alert>
      ) : null}

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
    </>
  );
}

export default AddRootFolder;
