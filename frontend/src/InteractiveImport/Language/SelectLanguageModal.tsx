import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import Language from 'Language/Language';
import SelectLanguageModalContent from './SelectLanguageModalContent';

interface SelectLanguageModalProps {
  isOpen: boolean;
  languageIds: number[];
  modalTitle: string;
  onLanguagesSelect(languages: Language[]): void;
  onModalClose(): void;
}

function SelectLanguageModal(props: SelectLanguageModalProps) {
  const { isOpen, languageIds, modalTitle, onLanguagesSelect, onModalClose } =
    props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose} size={sizes.MEDIUM}>
      <SelectLanguageModalContent
        languageIds={languageIds}
        modalTitle={modalTitle}
        onLanguagesSelect={onLanguagesSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectLanguageModal;
