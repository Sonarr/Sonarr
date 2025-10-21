import React from 'react';
import Modal from 'Components/Modal/Modal';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import { sizes } from 'Helpers/Props';
import InteractiveSearchPayload from 'InteractiveSearch/InteractiveSearchPayload';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import { ReleaseEpisode } from 'typings/Release';
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
  grabError?: string;
  searchPayload: InteractiveSearchPayload;
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
    searchPayload,
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
        searchPayload={searchPayload}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default OverrideMatchModal;
