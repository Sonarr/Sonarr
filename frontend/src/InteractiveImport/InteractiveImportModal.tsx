import React, { useCallback, useEffect, useState } from 'react';
import Modal from 'Components/Modal/Modal';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import InteractiveImportSelectFolderModalContent from './Folder/InteractiveImportSelectFolderModalContent';
import InteractiveImportModalContent from './Interactive/InteractiveImportModalContent';

interface InteractiveImportModalProps {
  isOpen: boolean;
  folder?: string;
  downloadId?: string;
  modalTitle?: string;
  onModalClose(): void;
}

function InteractiveImportModal(props: InteractiveImportModalProps) {
  const {
    isOpen,
    folder,
    downloadId,
    modalTitle = translate('ManualImport'),
    onModalClose,
    ...otherProps
  } = props;

  const [folderPath, setFolderPath] = useState<string | undefined>(folder);
  const previousIsOpen = usePrevious(isOpen);

  const onFolderSelect = useCallback(
    (path: string) => {
      setFolderPath(path);
    },
    [setFolderPath]
  );

  useEffect(() => {
    setFolderPath(folder);
  }, [folder, setFolderPath]);

  useEffect(() => {
    if (previousIsOpen && !isOpen) {
      setFolderPath(folder);
    }
  }, [folder, previousIsOpen, isOpen, setFolderPath]);

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_LARGE}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      {folderPath || downloadId ? (
        <InteractiveImportModalContent
          {...otherProps}
          folder={folderPath}
          downloadId={downloadId}
          modalTitle={modalTitle}
          onModalClose={onModalClose}
        />
      ) : (
        <InteractiveImportSelectFolderModalContent
          {...otherProps}
          modalTitle={modalTitle}
          onFolderSelect={onFolderSelect}
          onModalClose={onModalClose}
        />
      )}
    </Modal>
  );
}

export default InteractiveImportModal;
