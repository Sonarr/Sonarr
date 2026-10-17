import React, { useRef } from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearch from 'InteractiveSearch/InteractiveSearch';
import { useClearReleasesOnUnmount } from 'InteractiveSearch/useReleases';
import formatSeason from 'Season/formatSeason';
import translate from 'Utilities/String/translate';
import styles from './SeasonInteractiveSearchModalContent.module.css';

export interface SeasonInteractiveSearchModalContentProps {
  episodeCount: number;
  seriesId: number;
  seasonNumber: number;
  onModalClose(): void;
}

function SeasonInteractiveSearchModalContent({
  episodeCount,
  seriesId,
  seasonNumber,
  onModalClose,
}: SeasonInteractiveSearchModalContentProps) {
  useClearReleasesOnUnmount({ seriesId, seasonNumber });
  const modalBodyRef = useRef<HTMLDivElement>(null);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {seasonNumber === null
          ? translate('InteractiveSearchModalHeader')
          : translate('InteractiveSearchModalHeaderSeason', {
              season: formatSeason(seasonNumber) as string,
            })}
      </ModalHeader>

      <ModalBody ref={modalBodyRef} scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearch
          type="season"
          searchPayload={{
            seriesId,
            seasonNumber,
          }}
          scrollerRef={modalBodyRef}
        />
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div>
          {translate('EpisodesInSeason', {
            episodeCount,
          })}
        </div>

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeasonInteractiveSearchModalContent;
