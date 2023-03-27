import React from 'react';
import Modal from 'Components/Modal/Modal';
import SelectEpisodeModalContent, {
  SelectedEpisode,
} from './SelectEpisodeModalContent';

interface SelectEpisodeModalProps {
  isOpen: boolean;
  selectedIds: number[] | string[];
  seriesId: number;
  seasonNumber: number;
  selectedDetails?: string;
  isAnime: boolean;
  modalTitle: string;
  onEpisodesSelect(selectedEpisodes: SelectedEpisode[]): void;
  onModalClose(): void;
}

function SelectEpisodeModal(props: SelectEpisodeModalProps) {
  const {
    isOpen,
    selectedIds,
    seriesId,
    seasonNumber,
    selectedDetails,
    isAnime,
    modalTitle,
    onEpisodesSelect,
    onModalClose,
  } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectEpisodeModalContent
        selectedIds={selectedIds}
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        selectedDetails={selectedDetails}
        isAnime={isAnime}
        modalTitle={modalTitle}
        onEpisodesSelect={onEpisodesSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectEpisodeModal;
