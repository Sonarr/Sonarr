import React from 'react';
import Modal from 'Components/Modal/Modal';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import { sizes } from 'Helpers/Props';
import ReleaseEpisode from 'InteractiveSearch/ReleaseEpisode';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import OverrideMatchModalContent from './OverrideMatchModalContent';

interface OverrideMatchModalProps {
  isOpen: boolean;
  title: string;
  indexerId: number;
  guid: string;
  seriesId?: number;
  seasonNumber?: number;
  episodes: ReleaseEpisode[];
  languages: Language[];
  quality: QualityModel;
  protocol: DownloadProtocol;
  isGrabbing: boolean;
  grabError: string;
  onModalClose(): void;
}

function OverrideMatchModal(props: OverrideMatchModalProps) {
  const {
    isOpen,
    title,
    indexerId,
    guid,
    seriesId,
    seasonNumber,
    episodes,
    languages,
    quality,
    protocol,
    isGrabbing,
    grabError,
    onModalClose,
  } = props;

  return (
    <Modal isOpen={isOpen} size={sizes.LARGE} onModalClose={onModalClose}>
      <OverrideMatchModalContent
        title={title}
        indexerId={indexerId}
        guid={guid}
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        episodes={episodes}
        languages={languages}
        quality={quality}
        protocol={protocol}
        isGrabbing={isGrabbing}
        grabError={grabError}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default OverrideMatchModal;
