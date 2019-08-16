import PropTypes from 'prop-types';
import React from 'react';
import { scrollDirections } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';
import SeasonNumber from 'Season/SeasonNumber';

function SeasonInteractiveSearchModalContent(props) {
  const {
    seriesId,
    seasonNumber,
    onModalClose
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Interactive Search  {seasonNumber != null && <SeasonNumber seasonNumber={seasonNumber} />}
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
          Close
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
