import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { Season } from 'Series/Series';
import { createSeriesSelectorForHook } from 'Store/Selectors/createSeriesSelector';
import translate from 'Utilities/String/translate';
import SelectSeasonRow from './SelectSeasonRow';

interface SelectSeasonModalContentProps {
  seriesId?: number;
  modalTitle: string;
  onSeasonSelect(seasonNumber: number): void;
  onModalClose(): void;
}

function SelectSeasonModalContent(props: SelectSeasonModalContentProps) {
  const { seriesId, modalTitle, onSeasonSelect, onModalClose } = props;
  const series = useSelector(createSeriesSelectorForHook(seriesId));
  const seasons = useMemo<Season[]>(() => {
    return series.seasons.slice(0).reverse();
  }, [series]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectSeasonModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody>
        {seasons.map((item) => {
          return (
            <SelectSeasonRow
              key={item.seasonNumber}
              seasonNumber={item.seasonNumber}
              onSeasonSelect={onSeasonSelect}
            />
          );
        })}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectSeasonModalContent;
