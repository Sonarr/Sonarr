import PropTypes from 'prop-types';
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

function SeasonInteractiveSearchModalContent(props) {
  const {
    seriesId,
    seasonNumber,
    onModalClose
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {seasonNumber === null ?
          translate('InteractiveSearchModalHeader') :
          translate('InteractiveSearchModalHeaderSeason', { season: formatSeason(seasonNumber) })
        }
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearchConnector
          type="season"
          searchPayload={{
            seriesId,
            seasonNumber
          }}
        />
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>
          {translate('Close')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

SeasonInteractiveSearchModalContent.propTypes = {
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SeasonInteractiveSearchModalContent;
