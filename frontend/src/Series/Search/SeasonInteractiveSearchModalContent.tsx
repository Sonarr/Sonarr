import React from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';
import formatSeason from 'Season/formatSeason';
import translate from 'Utilities/String/translate';

interface SeasonInteractiveSearchModalContentProps {
  seriesId: number;
  seasonNumber: number;
  onModalClose(): void;
}

function SeasonInteractiveSearchModalContent(
  props: SeasonInteractiveSearchModalContentProps
) {
  const { seriesId, seasonNumber, onModalClose } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {seasonNumber === null
          ? translate('InteractiveSearchModalHeader')
          : translate('InteractiveSearchModalHeaderSeason', {
              season: formatSeason(seasonNumber) as string,
            })}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearchConnector
          type="season"
          searchPayload={{
            seriesId,
            seasonNumber,
          }}
        />
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeasonInteractiveSearchModalContent;
