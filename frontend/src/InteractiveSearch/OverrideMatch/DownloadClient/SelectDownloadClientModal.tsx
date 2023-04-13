import React from 'react';
import Modal from 'Components/Modal/Modal';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import { sizes } from 'Helpers/Props';
import SelectDownloadClientModalContent from './SelectDownloadClientModalContent';

interface SelectDownloadClientModalProps {
  isOpen: boolean;
  protocol: DownloadProtocol;
  modalTitle: string;
  onDownloadClientSelect(downloadClientId: number): void;
  onModalClose(): void;
}

function SelectDownloadClientModal(props: SelectDownloadClientModalProps) {
  const { isOpen, protocol, modalTitle, onDownloadClientSelect, onModalClose } =
    props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose} size={sizes.MEDIUM}>
      <SelectDownloadClientModalContent
        protocol={protocol}
        modalTitle={modalTitle}
        onDownloadClientSelect={onDownloadClientSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectDownloadClientModal;
